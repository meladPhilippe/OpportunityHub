# OpportunityHub Domain Structure and Principles

## 1. Purpose

This document summarizes the business model, domain boundaries, DDD principles, persistence decisions, and workflow concepts established for `OpportunityHub.Domain`.

The central principle is:

> **Identity, persistence tracking, and relationships are not the same thing. Model each only when the domain actually needs it.**

The domain model should describe what the business object needs to **know, protect, and control**. EF Core can handle additional persistence concerns without forcing those concerns into the domain model.

---

# 2. Core Domain Structure

The main business hierarchy is:

```text
Opportunity
    │
    └── OpportunityVersion
            │
            └── Submission
                    │
                    ├── ModificationRequest
                    │       └── ModificationRequestItem
                    │
                    ├── ModificationRejection
                    │
                    └── FinalRejection
```

However, not every database relationship should become a domain navigation property.

The domain model distinguishes between:

* Aggregate roots
* Child entities
* Association objects
* Reference data
* Value objects
* Workflow/activity records

---

# 3. Aggregate Boundaries

## Opportunity

`Opportunity` is responsible for the lifecycle of the opportunity itself.

It controls whether a **new submission/version** can be created.

```text
Opportunity
    │
    ├── controls opportunity lifecycle
    │
    └── controls whether a NEW Submission is allowed
```

It should not necessarily control every decision made on an existing submission.

---

## OpportunityVersion

An `OpportunityVersion` represents a specific version/snapshot of the opportunity.

```text
Opportunity
    │
    ├── Version 1
    │      └── Submission(s)
    │
    ├── Version 2
    │      └── Submission(s)
    │
    └── Version 3
           └── Submission(s)
```

The version owns its version-specific content and child objects such as:

* Features
* KPIs
* Key achievements
* Channel associations
* Sector associations

---

## Submission

`Submission` is the aggregate root for the workflow decision being made against a specific submission.

The key rule is:

> **Opportunity controls whether a new submission is allowed. Submission controls what decision can be made on the current submission.**

Therefore:

```text
Opportunity
    │
    └── Can a NEW submission be created?
                    │
                    ▼
               Submission
                    │
                    ├── RequestModification()
                    │
                    ├── RejectModification()
                    │
                    └── RejectOpportunity()
```

The aggregate should expose behavior rather than allowing arbitrary state mutation.

Prefer:

```csharp
submission.RequestModification(...);
```

over:

```csharp
submission.ModificationRequest = request;
```

The aggregate creates and controls its children.

---

# 4. Modification Workflow

A modification request is not the same thing as a rejection.

The workflow is:

```text
                    Submission
                        │
                        ▼
              ┌─────────────────┐
              │     Decision    │
              └─────────────────┘
                 │       │      │
                 │       │      │
                 ▼       ▼      ▼
              Approve  Request  Final
                       Changes   Reject
                          │
                          ▼
                 ModificationRequest
```

When modifications are requested:

```csharp
Submission.RequestModification(...)
```

```text
    │
    └── creates ModificationRequest
                    │
                    └── ModificationRequestItem(s)
```

When a modification request is rejected:

```csharp
Submission.RejectModification(...)
```

```text
    │
    └── creates ModificationRejection
```

When the opportunity itself is finally rejected:

```csharp
Submission.RejectOpportunity(...)
```

```text
    │
    └── creates FinalRejection
```

These represent different business concepts.

```text
ModificationRequest
    │
    └── asks for changes

ModificationRejection
    │
    └── rejects a modification request
        where that concept applies

FinalRejection
    │
    └── permanently rejects the opportunity
```

The domain should therefore model these as separate concepts instead of treating every rejection as the same state.

---

# 5. First Publication Workflow

A particularly important distinction exists for the **first publication**.

For first publication:

* The opportunity has never been approved.
* The opportunity has never been published.
* A manager/specialist can request modifications.
* Requesting modifications does **not** mean the opportunity is rejected.
* The submission can subsequently be approved/published.
* Modifications can be requested again.
* The final rejection is the terminal rejection.

Therefore:

```text
First Publication Submission
            │
            ▼
       ┌─────────────┐
       │    Review   │
       └─────────────┘
        │     │     │
        │     │     │
        ▼     ▼     ▼
     Approve Request Final
             Changes Reject
                │
                ▼
       ModificationRequest
                │
                ▼
          Modify Version
                │
                ▼
         Submit Again
                │
                ▼
             Review
```

There is **no separate "reject modification" outcome that terminates the first-publication workflow**.

The distinction is:

```text
ModificationRequest
    │
    └── asks for changes

ModificationRejection
    │
    └── rejects a modification request
        where that concept applies

FinalRejection
    │
    └── permanently rejects the opportunity
```

---

# 6. Domain Behavior Over Setters

DDD entities should not expose public setters merely to make state changes convenient.

Prefer:

```csharp
public Guid OpportunityVersionId { get; private set; }
```

instead of:

```csharp
public Guid OpportunityVersionId { get; set; }
```

More importantly, state transitions should be expressed through domain behavior:

```text
Submission.RequestModification()
    │
    ├── validates business rules
    ├── creates ModificationRequest
    └── changes submission state

Submission.RejectModification()
    │
    ├── validates business rules
    ├── creates ModificationRejection
    └── changes submission state

Submission.RejectOpportunity()
    │
    ├── validates business rules
    ├── creates FinalRejection
    └── terminates workflow
```

This keeps invariants inside the aggregate instead of spreading them across application services.

---

# 7. Invariants

An **invariant** is a business rule that must always remain true for the domain model to be valid.

For example:

```text
Submission
    │
    ├── Current workflow state
    │
    ├── ModificationRequest
    │
    ├── ModificationRejection
    │
    └── FinalRejection
```

The submission should prevent invalid combinations and transitions.

For example, application code should not be able to arbitrarily do:

```csharp
submission.Status = SomeInvalidStatus;
submission.ModificationRequest = request;
submission.FinalRejection = rejection;
```

Instead:

```csharp
submission.RequestModification(...);
```

allows the aggregate to decide whether that operation is valid.

---

# 8. Parent and Child Relationships

A database relationship does **not** automatically mean both sides belong in the domain model.

If:

```text
Submission
    │
    └── ModificationRequest
```

and `Submission` is the aggregate root, then the parent already establishes the relationship.

The child does not necessarily need:

```csharp
public Guid SubmissionId { get; private set; }
```

in the domain model.

Instead:

```csharp
public sealed class Submission
{
    private ModificationRequest? _modificationRequest;

    public void RequestModification(...)
    {
        _modificationRequest = new ModificationRequest(...);
    }
}
```

The aggregate root manages the child.

---

# 9. EF Core Shadow Foreign Keys

EF Core can maintain a foreign key without exposing it in the domain model.

For example, the domain can contain:

```csharp
public sealed class Submission
{
    public Guid OpportunityVersionId { get; private set; }
}
```

while EF Core can maintain:

```text
Submission
    ├── OpportunityVersionId
    └── OpportunityId   ← shadow property
```

The EF configuration can define:

```csharp
builder.Property<Guid>("OpportunityId");
```

The result is:

```text
DOMAIN

Opportunity
    │
    └── OpportunityVersion
            │
            └── Submission


DATABASE

Opportunity
    │
    └── OpportunityVersion
            │
            └── Submission
                  ├── OpportunityId
                  └── OpportunityVersionId
```

The database can therefore have information required for persistence/querying without forcing the domain object to expose it.

---

# 10. OpportunityId Duplication

Having both:

```text
OpportunityId
OpportunityVersionId
```

on `Submission` may be useful at the database level.

It allows efficient queries such as:

```sql
SELECT *
FROM Submission
WHERE OpportunityId = @OpportunityId;
```

However:

> **A persistence optimization should not dictate the domain model.**

Therefore:

```text
DOMAIN

Opportunity
    └── OpportunityVersion
            └── Submission


DATABASE

Opportunity
    └── OpportunityVersion
            └── Submission
                  ├── OpportunityId
                  └── OpportunityVersionId
```

`OpportunityId` can be an EF Core shadow property if the domain itself does not need it.

---

# 11. Composite Foreign Keys and Database Integrity

Duplicating `OpportunityId` and `OpportunityVersionId` creates a potential consistency problem.

For example:

```text
Opportunity A
    │
    └── Version 1

Opportunity B
    │
    └── Version 2
```

An invalid record could theoretically contain:

```text
Submission
    OpportunityId        = A
    OpportunityVersionId = Version 2
```

which would associate the submission with:

```text
Opportunity A
Version belonging to Opportunity B
```

The database can prevent this using a composite foreign key:

```text
OpportunityVersion
    PK:
        Id
        OpportunityId
          ▲
          │
          │ FK
          │
Submission
    OpportunityId
    OpportunityVersionId

Constraint:

(OpportunityId, OpportunityVersionId)
              │
              ▼
OpportunityVersion
(OpportunityId, Id)
```

This gives the database an additional integrity guarantee.

---

# 12. Domain Object vs Entity vs Value Object

`DomainObject` is a broad concept.

It means:

> An object that represents something meaningful in the business domain.

It does not necessarily mean the object is an entity.

A conceptual hierarchy can be:

```text
DomainObject
    │
    ├── Entity
    │
    ├── Value Object
    │
    ├── Domain Event
    │
    └── Other Domain Concepts
```

## Entity

An entity has identity that remains meaningful throughout its lifecycle.

Examples:

```text
Opportunity
OpportunityVersion
Submission
OpportunityVersionFeature
OpportunityVersionKpi
```

Typically:

```csharp
public Guid Id { get; protected set; }
```

---

## Value Object

A value object is defined by its values rather than by identity.

Examples:

```text
LocalizedText
OpportunityVersionContent
FeatureContent
KpiContent
```

For example:

```csharp
public sealed class LocalizedText
{
    public string En { get; private set; }
    public string? Ar { get; private set; }
}
```

Two `LocalizedText` objects containing the same values represent the same value.

---

# 13. Association Objects

Not every many-to-many relationship deserves a domain entity.

Consider:

```text
OpportunityVersion
        │
        ├──────── Channel
        │
        └──────── Sector
```

If the relationship only means:

> "This version is associated with this channel."

then the relationship can be represented as a database association.

For example:

```text
OpportunityVersionChannel

    OpportunityVersionId
    ChannelId
```

It does not necessarily need a domain identity.

---

## When an Association Becomes a Domain Concept

An association becomes a domain object when the relationship itself has meaningful business information.

For example:

```text
OpportunityVersion
        │
        └── ChannelAssociation
                ├── ChannelId
                ├── IsPrimary
                ├── EffectiveFrom
                └── SortOrder
```

Now the relationship has business meaning.

The distinction is:

```text
Simple relationship
    │
    └── EF/database association

Meaningful relationship
    │
    └── Association domain object
```

---

# 14. Reference Data

Reference data is another important category.

Examples:

```text
Channel
Sector
RejectionReason
```

The domain may only need the identifier:

```csharp
public Guid ChannelId { get; private set; }
```

or:

```csharp
public int RejectionReasonId { get; private set; }
```

There is no requirement to load the complete reference entity into the aggregate.

For example:

```text
OpportunityVersion
    │
    └── ChannelId
            │
            ▼
         Channel
       Reference Data
```

The aggregate can work with the ID when it does not need additional channel behavior.

---

# 15. Child Entity vs Association Object vs Reference Object

These concepts describe different roles.

| Concept            | Has Identity?                   | Business Meaning                         | Typical Example             |
| ------------------ | ------------------------------- | ---------------------------------------- | --------------------------- |
| Entity             | Yes                             | Independent business identity            | `OpportunityVersion`        |
| Child Entity       | Yes                             | Meaningful object inside aggregate       | `OpportunityVersionFeature` |
| Association Object | Usually no independent identity | Represents a meaningful relationship     | `OpportunityVersionChannel` |
| Reference Data     | Yes                             | Independent lookup/reference information | `Channel`, `Sector`         |
| Value Object       | No                              | Defined by its values                    | `LocalizedText`             |

The important question is not:

> "Does the database have a table?"

The important question is:

> **"What does this object mean to the business?"**

---

# 16. Persistence Concerns vs Domain Concerns

The domain model should not be shaped solely around EF Core.

There are several techniques for keeping persistence concerns separate.

## Technique 1 — Shadow Properties

Use shadow properties for information that is useful to EF/database persistence but not required by domain behavior.

Examples:

```text
OpportunityId
Audit metadata
Soft-delete flags
Concurrency tokens
Database-specific metadata
```

Example:

```csharp
builder.Property<Guid>("OpportunityId");
```

The property exists in the EF model but not in the CLR class.

---

## Technique 2 — Backing Fields

Aggregate collections should often use backing fields.

Example:

```csharp
private readonly List<ModificationRequestItem> _items = new();

public IReadOnlyCollection<ModificationRequestItem> Items =>
    _items.AsReadOnly();
```

The domain exposes:

```text
IReadOnlyCollection
```

while EF can map:

```text
_items
```

This prevents external code from arbitrarily modifying the collection.

The aggregate controls:

* Add
* Remove
* Replace
* Validate

---

## Technique 3 — Private Setters

Sometimes the domain genuinely needs a property.

In that case, expose it with a private setter:

```csharp
public SubmissionType SubmissionType { get; private set; }
```

This means:

* EF can populate the property.
* The domain can change it.
* Application code cannot arbitrarily assign it.

Private setters are therefore useful when the property is genuinely part of the domain state.

---

## Technique 4 — Separate EF Core Configuration

Persistence configuration should remain outside the domain classes.

For example:

```csharp
public sealed class SubmissionConfiguration
    : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        // mapping
    }
}
```

This keeps things such as:

* Table names
* Column names
* Indexes
* Foreign keys
* Delete behavior
* Shadow properties
* Precision
* SQL Server-specific configuration

out of the domain model.

This is strongly preferred for `OpportunityHub`.

---

## Technique 5 — Persistence-Specific Types

For more complex systems, the persistence model can be completely separated:

```text
Domain
    Submission

Infrastructure
    SubmissionRecord

Database
    Submission
```

Mapping then becomes:

```text
SubmissionRecord
       │
       ▼
   Database
```

and:

```text
SubmissionRecord
       │
       ▼
   Submission
```

This provides the strongest separation but introduces more mapping and maintenance code.

For `OpportunityHub`, this should be used only when the additional separation provides a real benefit.

---

# 17. Tracking Types and Domain Identity

Tracking and identity are separate concepts.

For example:

```text
EntityIdentity
    │
    └── Id

CreationTrackedEntity
    │
    ├── Id
    ├── CreatedAtUtc
    └── CreatedBy

ChangeTrackedEntity
    │
    ├── Id
    ├── CreatedAtUtc
    ├── CreatedBy
    ├── UpdatedAtUtc
    └── UpdatedBy
```

The distinction is:

```text
Identity
    = Who is this object?

Creation tracking
    = Who created it and when?

Change tracking
    = Who last changed it and when?
```

These should not automatically be confused with business behavior.

---

# 18. AuditHistory Is Different

`AuditHistory` is **not simply a child record of Submission**.

It represents a workflow/activity record associated with the broader opportunity lifecycle.

Therefore:

```text
Opportunity
    │
    └── OpportunityVersion
            │
            └── Submission
```

while:

```text
AuditHistory
    ├── OpportunityId
    ├── OpportunityVersionId
    ├── SubmissionId?
    ├── ActivitySequenceNumber
    ├── ActivityType
    ├── RelatedEntityType?
    └── RelatedEntityId?
```

The important meaning is:

> "This activity happened for Opportunity X, Version Y, optionally in the context of Submission Z."

Therefore the IDs are part of the meaning of the audit record, not merely EF foreign keys.

---

# 19. AuditHistory Context

The audit structure supports both submission-specific and broader workflow activities.

```text
Opportunity
    │
    └── OpportunityVersion
            │
            └── Submission
                    │
                    └── Activity
```

But an activity may also be:

```text
Opportunity
    │
    └── OpportunityVersion
            │
            └── AuditHistory
```

without requiring a `Submission`.

Therefore:

```csharp
public Guid OpportunityId { get; private set; }

public Guid OpportunityVersionId { get; private set; }

public Guid? SubmissionId { get; private set; }
```

is meaningful domain information.

---

# 20. Related Entity Type and Related Entity ID

`AuditHistory` can optionally point to another domain object:

```text
RelatedEntityType
RelatedEntityId
```

Conceptually:

```text
AuditHistory
      │
      ├── ActivityType
      │
      └── Related Entity
              │
              ├── RelatedEntityType
              └── RelatedEntityId
```

For example, an activity might relate to:

```text
ModificationRequest
ModificationRequestItem
FinalRejection
ModificationRejection
```

The audit record does not necessarily need a strongly typed navigation to every possible related object.

Instead:

```text
RelatedEntityType = "ModificationRequest"
RelatedEntityId   = <Guid>
```

allows the application to identify the contextual object and retrieve additional information when required.

This keeps `AuditHistory` generic while still providing a link to the business object responsible for the activity.

---

# 21. Audit Activity Example

A workflow sequence could look like:

```text
Submission Created
        │
        ▼
Manager Review Started
        │
        ▼
Modification Requested
        │
        ├── RelatedEntityType = ModificationRequest
        └── RelatedEntityId   = Request.Id
        │
        ▼
Specialist Modified Opportunity
        │
        ▼
Submitted Again
        │
        ▼
Manager Approved
        │
        ▼
Published
```

The audit history records the activity independently from the aggregate's current state.

---

# 22. Audit Sequence

`ActivitySequenceNumber` provides an ordered history:

```text
1  SubmissionCreated
2  ReviewStarted
3  ModificationRequested
4  ModificationCompleted
5  Resubmitted
6  Approved
7  Published
```

This is different from a normal database identity.

The `Id` answers:

> Which audit record is this?

The sequence answers:

> In what order did this activity occur?

Therefore both concepts can coexist.

---

# 23. Content Value Objects

The opportunity version content is represented as a value-object-style structure:

```text
OpportunityVersionContent
    │
    ├── OpportunityName
    ├── NationalImpact
    ├── Description
    ├── WebsiteUrl
    ├── LogoReference
    ├── BannerReference
    ├── CompanyName
    ├── CompanyWebsiteUrl
    ├── AdoptedBy
    ├── Beneficiaries
    ├── KsaAdoptingEntitiesCount
    ├── OpportunityOwnerName
    ├── OpportunityOwnerEmail
    ├── OpportunityOwnerPhone
    ├── ChannelIds
    ├── SectorIds
    ├── Features
    ├── KeyAchievements
    └── Kpis
```

# 24. LocalizedText

Localized values are represented by:

```text
LocalizedText
    ├── En
    └── Ar
```

The English value is required:

```csharp
ArgumentException.ThrowIfNullOrWhiteSpace(en);
```

The value can be changed through domain behavior:

```csharp
localizedText.Set(en, ar);
```

rather than exposing public setters.

---

# 25. Feature, KPI and Key Achievement

These are different from simple reference data.

They represent meaningful content belonging to an opportunity version.

Conceptually:

```text
OpportunityVersion
    │
    ├── Features
    ├── KPIs
    └── KeyAchievements
```

Their content can be represented through value-object-like input models:

```text
FeatureContent
KpiContent
KeyAchievementContent
```

while the persisted domain entities can retain their own identity where required.

For example:

```text
OpportunityVersionFeature
    ├── Id
    ├── Title
    ├── IconReference
    ├── SortOrder
    └── DisplayOnWebsite
```

The important distinction is:

```text
Feature
    = meaningful child entity

IconReference
    = reference value

FeatureContent
    = content/value representation
```

---

# 26. Channels and Sectors

Channels and sectors are reference data.

The opportunity version does not need to own the actual `Channel` or `Sector` entity.

Instead:

```text
OpportunityVersion
    │
    ├── ChannelIds
    └── SectorIds
```

The database can maintain:

```text
OpportunityVersionChannel
    OpportunityVersionId
    ChannelId

OpportunityVersionSector
    OpportunityVersionId
    SectorId
```

This prevents the aggregate from becoming responsible for unrelated reference-data objects.

---

# 27. Creation of Child Objects

A child object should generally be created by the aggregate that owns it.

For example:

```text
Submission
    │
    └── RequestModification()
             │
             ▼
      ModificationRequest
```

rather than:

```text
Application Service
    │
    ├── new ModificationRequest()
    │
    └── submission.ModificationRequest = request
```

The first approach keeps the invariant inside the aggregate.

---

# 28. The Aggregate Root Knows Its Children

A useful rule is:

> **The aggregate root knows and controls its children. The child does not necessarily need to know its parent.**

Therefore:

```text
Submission
    │
    └── ModificationRequest
```

does not automatically require:

```csharp
ModificationRequest.SubmissionId
```

in the domain model.

EF Core can maintain the relationship through a shadow foreign key.

This produces a clean separation:

```text
DOMAIN

Submission
    └── ModificationRequest


DATABASE

Submission
    │
    └── ModificationRequest
            └── SubmissionId
```

---

# 29. When a Child Should Have Parent Identity

The exception is when the parent identity is itself meaningful domain information.

`AuditHistory` is the key example.

It means:

```text
This activity occurred for:
    Opportunity X
    Version Y
    optionally Submission Z
```

Therefore:

```text
AuditHistory
    ├── OpportunityId
    ├── OpportunityVersionId
    └── SubmissionId?
```

is appropriate.

The question should always be:

> **Does the parent identity help express the meaning or behavior of this domain object?**

If yes, keep it.

If no, consider a shadow FK.

---

# 30. Domain vs Database View

A useful mental model for `OpportunityHub` is:

```text
                DOMAIN
                  │
       ┌──────────┴──────────┐
       │                     │
   Business Rules        Business Meaning
       │                     │
       └──────────┬──────────┘
                  │
                  ▼
             Domain Model
                  │
                  │
            EF Core Mapping
                  │
                  ▼
               DATABASE
                  │
       ┌──────────┼──────────┐
       │          │          │
      FKs      Indexes   Constraints
       │          │          │
   Shadow FKs   Query     Integrity
```

The database may contain more information than the domain model.

That is acceptable.

---

# 31. Decision Framework

When deciding whether to add a property or relationship to a domain entity, ask:

### Question 1

**Does the business object need this information to make a decision?**

If yes:

```text
Keep it in the domain.
```

If no:

```text
Consider keeping it persistence-only.
```

### Question 2

**Does the object need to control the relationship?**

If yes:

```text
Model the relationship/domain behavior.
```

If no:

```text
Let EF/database handle it.
```

### Question 3

**Does the relationship itself have business meaning?**

If no:

```text
Association table / EF mapping.
```

If yes:

```text
Association domain object.
```

### Question 4

**Does the child have meaningful independent identity?**

If yes:

```text
Child entity.
```

If no:

```text
Value object or association object.
```

---

# 32. Practical Decision Tree

```text
                    Database Relationship
                            │
                            ▼
                Does the domain need it?
                       /          \
                     No            Yes
                     │              │
                     ▼              ▼
              EF mapping only   What kind?
                                   │
                    ┌──────────────┼───────────────┐
                    │              │               │
                    ▼              ▼               ▼
                 Reference      Child Entity    Association
                    │              │               │
                    ▼              ▼               ▼
                  ID only      Has identity?    Has business
                                Owns behavior?    meaning?
                                    │               │
                                    ▼               ▼
                              Domain entity    Domain association
```

---

# 33. Recommended OpportunityHub Principles

The following principles should be applied consistently throughout `OpportunityHub.Domain`.

## Principle 1 — Behavior over setters

```text
Bad:

entity.Status = ...

Good:

entity.Approve()
entity.RequestModification()
entity.RejectOpportunity()
```

---

## Principle 2 — Aggregate boundaries matter

```text
Opportunity
    └── controls new submissions

Submission
    └── controls current submission decisions
```

Do not move workflow decisions to unrelated objects.

---

## Principle 3 — Database relationships are not domain relationships

A foreign key existing in SQL Server does not automatically require a navigation property in the domain.

---

## Principle 4 — Keep persistence concerns out of the domain when possible

Use:

* Shadow properties
* Backing fields
* Private setters
* Separate EF configurations
* Persistence-specific types when justified

---

## Principle 5 — Reference data should normally be represented by IDs

Do not load the entire reference entity into an aggregate unless the domain requires it.

---

## Principle 6 — Meaningful relationships deserve domain concepts

A pure join table does not automatically need a domain object.

If the relationship has business behavior or meaningful attributes, model it.

---

## Principle 7 — Children are controlled by the aggregate root

```text
Aggregate Root
      │
      └── creates / changes / removes children
```

The application should not directly manipulate aggregate internals.

---

## Principle 8 — Parent IDs are not automatically required

A child does not need a parent ID merely because EF needs a foreign key.

Use shadow FKs where appropriate.

---

## Principle 9 — Audit is contextual, not necessarily hierarchical

`AuditHistory` belongs to the opportunity lifecycle and may reference:

```text
Opportunity
OpportunityVersion
Submission
Related Entity
```

It should not be forced into a simple `Submission -> AuditHistory` child hierarchy.

---

## Principle 10 — Database integrity still matters

DDD does not replace database constraints.

The domain protects business invariants while the database protects data integrity.

Both layers should work together.

---

# 34. Final Mental Model

The overall model can be summarized as:

```text
                         Opportunity
                              │
                              ▼
                    OpportunityVersion
                              │
             ┌────────────────┼─────────────────┐
             │                │                 │
             ▼                ▼                 ▼
        Content          Reference Data      Submission
             │             │                 │
             │             ├── Channels      │
             │             └── Sectors       │
             │                               │
             ├── Features                    ├── ModificationRequest
             ├── KPIs                        │      └── Items
             └── KeyAchievements             │
                                             ├── ModificationRejection
                                             │
                                             └── FinalRejection


                  Opportunity Lifecycle
                          │
                          ▼
                    AuditHistory
                          │
          ┌───────────────┼────────────────┐
          │               │                │
          ▼               ▼                ▼
    Opportunity    OpportunityVersion   Submission
                                             │
                                             ▼
                                      Related Entity
```

The core philosophy is:

```text
                    BUSINESS MEANING
                          │
                          ▼
                    DOMAIN MODEL
                          │
             ┌────────────┼────────────┐
             │            │            │
             ▼            ▼            ▼
          Entities    Value Objects   Behaviors
             │
             ▼
       Aggregate Rules
             │
             ▼
       Persistence Mapping
             │
             ▼
          EF Core
             │
             ▼
         SQL Server
```

## Final Rule

> **Do not model the database in the domain. Model the business, then map the business model to the database.**

Or, more specifically for `OpportunityHub`:

> **Identity tells us what an object is. Relationships tell us how objects are connected. Persistence tracking tells us how data is stored. Business behavior tells us what the domain allows. These concerns are related, but they are not the same thing.**
