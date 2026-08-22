# Domain-Driven Design Tutorial: Modeling a Workflow-Driven Business Domain

## 1. Introduction

This tutorial explains the Domain-Driven Design (DDD) concepts discussed while designing a workflow-driven business domain.

The examples use a generic business domain called **OpportunityHub**. The domain contains opportunities, versions, submissions, reviews, statuses, sub-statuses, and workflow actions.

The goal is not to apply DDD patterns mechanically. The goal is to understand:

- What belongs in the Domain layer
- What belongs in the Application layer
- What an Entity is
- What an Aggregate is
- What an Aggregate Root is
- What a Value Object is
- What a Domain Policy is
- When a Domain Service is appropriate
- Why workflow rules are domain knowledge
- Why an Application Resolver/Executor may be unnecessary
- What a consistency boundary means
- How all these concepts work together

---

# 2. The Big Picture

A useful high-level architecture is:

```text
API / UI
   |
   v
Application Layer
   |
   |  Load aggregate
   |  Invoke domain behavior
   |  Persist
   v
Domain Layer
   |
   +-- Aggregate Root
   +-- Entities
   +-- Value Objects
   +-- Domain Policies
   +-- Domain Exceptions
```

The dependency direction is:

```text
API
  |
  v
Application
  |
  v
Domain
```

The Domain should not depend on HTTP, controllers, API response models, or application-specific orchestration.

---

# 3. Domain vs Application

This is one of the most important DDD distinctions.

## Domain Layer

The Domain answers:

> What are the business rules and business behaviors?

Examples:

```text
Can this opportunity be submitted?
Can it be approved?
Can it be published?
What happens when it is submitted?
What status does it enter?
Can this workflow transition happen?
What invariants must always hold?
```

The Domain should contain business concepts such as:

```text
Opportunity
OpportunityVersion
WorkflowAction
WorkflowKey
WorkflowPolicy
```

## Application Layer

The Application layer answers:

> How do I execute a business use case?

Examples:

```text
Load Opportunity
Call Opportunity.SubmitForManagerReview()
Save changes
Return the result
```

The Application layer should orchestrate the use case, but it should not become the owner of the business rules.

A simple Application use case may look like:

```csharp
public async Task SubmitForManagerReview(
    Guid opportunityId,
    SubmitForManagerReviewCommand command,
    CancellationToken cancellationToken)
{
    var opportunity = await _repository.GetByIdAsync(
        opportunityId,
        cancellationToken);

    if (opportunity is null)
        throw new OpportunityNotFoundException(opportunityId);

    opportunity.SubmitForManagerReview(
        command.Content,
        command.UserId,
        command.UserName,
        command.EditSummary,
        DateTime.UtcNow);

    await _unitOfWork.SaveChangesAsync(cancellationToken);
}
```

Notice what is missing:

```text
No workflow resolver
No workflow strategy
No status manipulation
No business rule implementation
```

The Application layer asks the Domain to perform the operation.

---

# 4. What Is an Entity?

An Entity is a domain object that has a meaningful identity.

For example:

```csharp
public sealed class Opportunity
{
    public Guid OpportunityId { get; private set; }
}
```

The important characteristic is identity.

Two objects can have the same data but still represent different entities:

```text
Opportunity A
Id = 100

Opportunity B
Id = 200
```

Even if every other property is identical, they are different opportunities.

Entities can also have behavior.

For example:

```csharp
public void SubmitForManagerReview(...)
{
    ...
}
```

An Entity is therefore not merely a database row.

It represents a business concept with identity and behavior.

---

# 5. What Is an Aggregate?

An Aggregate is a group of related domain objects that are treated as one consistency boundary.

For example:

```text
Opportunity Aggregate
|
+-- Opportunity              <- Aggregate Root
|
+-- OpportunityVersion
|
+-- OpportunitySubmission
|
+-- OpportunityAuditHistory
|
+-- OpportunityInterest
```

The objects belong to one conceptual business unit.

The aggregate is not simply a collection of tables.

It defines which objects must remain consistent together when a business operation occurs.

---

# 6. What Does "Consistency Boundary" Mean?

This is a very important DDD term.

A consistency boundary means:

> Inside this boundary, the domain guarantees that the objects satisfy their required business invariants as one unit.

Consider:

```text
Opportunity
|
+-- Current Version
+-- Submission
+-- Audit History
```

When the opportunity is submitted for manager review, several things may happen:

```text
1. The current version is updated
2. The opportunity status changes
3. A submission is created
4. Audit history is created
```

The business may require these changes to be consistent.

You do not want a state such as:

```text
Opportunity = Pending Manager Review

but

Submission = missing
```

if the business rule says every submission must have a corresponding submission record.

The aggregate boundary says:

> These objects participate in one business consistency model.

That is what "consistency boundary" means.

---

# 7. Aggregate Root

An Aggregate Root is the entity that controls access to the aggregate.

In this example:

```text
Opportunity Aggregate
|
+-- Opportunity              <- Aggregate Root
|
+-- OpportunityVersion
+-- OpportunitySubmission
+-- OpportunityAuditHistory
```

`Opportunity` is the Aggregate Root.

Application code should generally interact with:

```csharp
opportunity.SubmitForManagerReview(...);
```

rather than directly modifying:

```csharp
opportunityVersion.Status = ...;
opportunity.Submissions.Add(...);
opportunityAuditHistory = ...;
```

The root protects the aggregate's invariants.

---

# 8. Aggregate vs Aggregate Root

These terms are related but not identical.

## Aggregate

The entire boundary:

```text
Opportunity
OpportunityVersion
OpportunitySubmission
OpportunityAuditHistory
```

## Aggregate Root

The entity at the boundary:

```text
Opportunity
```

So:

```text
Aggregate = the whole cluster

Aggregate Root = the entry point / controlling entity
```

In many designs, the Aggregate Root is itself an Entity.

Therefore:

```text
Opportunity
= Entity
= Aggregate Root
= root of the Opportunity Aggregate
```

But these are three different concepts:

```text
Entity       -> identity
Aggregate    -> consistency boundary
Aggregate Root -> entity controlling the aggregate boundary
```

---

# 9. Why Does the Aggregate Root Own Behavior?

Suppose the business operation is:

> Submit an opportunity for manager review.

The natural business sentence is:

> "The opportunity submits itself for manager review."

Therefore the behavior belongs naturally to the Aggregate Root:

```csharp
public void SubmitForManagerReview(...)
{
    ...
}
```

This method may update multiple objects inside the aggregate.

That is completely fine.

For example:

```text
Opportunity.SubmitForManagerReview()
        |
        +-- update OpportunityVersion
        |
        +-- change Opportunity status
        |
        +-- create Submission
        |
        +-- create AuditHistory
```

You do NOT need a Domain Service simply because multiple entities are changed.

They all belong to the same aggregate.

---

# 10. What Is a Value Object?

A Value Object represents a value rather than an identity.

For example:

```csharp
public readonly record struct WorkflowKey(
    OpportunityStatus Status,
    OpportunitySubStatus? SubStatus,
    WorkflowAction Action);
```

A WorkflowKey is identified by its values.

For example:

```text
Draft
null
SubmitForManagerReview
```

There is no meaningful `WorkflowKeyId`.

If two keys contain the same values, they represent the same value.

That is why a record struct is a natural representation.

---

# 11. Entity vs Value Object

Compare:

```text
Opportunity
```

with:

```text
WorkflowKey
```

Opportunity:

```text
Identity matters
```

WorkflowKey:

```text
Values matter
```

Therefore:

```text
Opportunity
    -> Entity

WorkflowKey
    -> Value Object
```

---

# 12. Workflow Modeling

The business domain has workflow concepts:

```text
Draft
Pending Manager Review
Pending Specialist Modification
Approved
Rejected
Published
Published Under Review
Unpublished
```

And actions:

```text
SubmitForManagerReview
Approve
Reject
RequestModification
Publish
Unpublish
```

The workflow rule can be represented as:

```text
Current Status
+
Current SubStatus
+
Requested Action
        |
        v
Allowed / Not Allowed
```

This is business knowledge.

Therefore the workflow definition belongs in the Domain.

---

# 13. WorkflowAction

```csharp
public enum WorkflowAction
{
    SubmitForManagerReview,
    Approve,
    Reject,
    RequestModification,
    Publish,
    Unpublish
}
```

This is a Domain concept because the actions are part of the business language.

They are not HTTP concepts.

The API may happen to transport the action, but the action itself belongs to the business domain.

---

# 14. WorkflowKey

```csharp
public readonly record struct WorkflowKey(
    OpportunityStatus Status,
    OpportunitySubStatus? SubStatus,
    WorkflowAction Action);
```

It represents:

```text
Current business state
+
Requested business action
```

For example:

```csharp
new WorkflowKey(
    OpportunityStatus.Draft,
    null,
    WorkflowAction.SubmitForManagerReview);
```

---

# 15. Workflow Policy / Definition

The workflow transition matrix is business knowledge.

For example:

```text
Draft
    + SubmitForManagerReview
        -> Allowed

PendingManagerReview
    + Approve
        -> Allowed

PendingManagerReview
    + Reject
        -> Allowed

Published
    + SubmitForManagerReview
        -> Not Allowed
```

This can be represented as a Domain Policy.

```csharp
public static class WorkflowPolicy
{
    private static readonly IReadOnlySet<WorkflowKey>
        AllowedTransitions =
        new HashSet<WorkflowKey>
        {
            new(
                OpportunityStatus.Draft,
                null,
                WorkflowAction.SubmitForManagerReview),

            new(
                OpportunityStatus.PendingManagerReview,
                null,
                WorkflowAction.Approve),

            new(
                OpportunityStatus.PendingManagerReview,
                null,
                WorkflowAction.Reject),

            new(
                OpportunityStatus.PendingManagerReview,
                null,
                WorkflowAction.RequestModification),

            new(
                OpportunityStatus.Approved,
                null,
                WorkflowAction.Publish),

            new(
                OpportunityStatus.Published,
                null,
                WorkflowAction.Unpublish)
        };

    public static bool IsAllowed(WorkflowKey key)
    {
        return AllowedTransitions.Contains(key);
    }

    public static IReadOnlyCollection<WorkflowAction>
        GetAllowedActions(
            OpportunityStatus status,
            OpportunitySubStatus? subStatus)
    {
        return AllowedTransitions
            .Where(x =>
                x.Status == status &&
                x.SubStatus == subStatus)
            .Select(x => x.Action)
            .ToArray();
    }
}
```

---

# 16. Is WorkflowPolicy a Domain Service?

No.

It is better described as a:

```text
Domain Policy
```

or:

```text
Domain Rule Definition
```

It is stateless business knowledge.

A Domain Service is a different concept.

---

# 17. What Is a Domain Service?

A Domain Service is useful when you have genuine domain behavior that does not naturally belong to one Entity, Value Object, or Aggregate Root.

The important phrase is:

> Does this behavior naturally belong to one domain object?

If yes, put it there.

If no, consider a Domain Service or Domain Policy.

Do NOT use this simplistic rule:

```text
Multiple entities = Domain Service
```

That is wrong.

An Aggregate Root can legitimately coordinate multiple entities inside its own aggregate.

---

# 18. Example: Behavior Belonging to the Aggregate Root

Suppose submission changes:

```text
Opportunity
OpportunityVersion
Submission
AuditHistory
```

All are inside the Opportunity Aggregate.

Therefore:

```csharp
opportunity.SubmitForManagerReview(...);
```

is appropriate.

No Domain Service is needed.

The root owns the operation because:

> The opportunity is the business subject performing the operation.

---

# 19. Example: Behavior That May Need a Domain Service

Imagine publishing requires rules involving three separate aggregates:

```text
Opportunity Aggregate
Organization Aggregate
Sector Aggregate
```

Business rule:

> An opportunity can be published only when the opportunity is approved, the organization has sufficient capacity, and the sector is eligible.

No single aggregate naturally owns the complete rule.

A domain policy/service could express:

```csharp
public sealed class PublishingPolicy
{
    public bool CanPublish(
        Opportunity opportunity,
        Organization organization,
        Sector sector)
    {
        return opportunity.IsApproved()
            && organization.HasCapacity()
            && sector.IsEligible();
    }
}
```

The policy coordinates a domain concept involving multiple aggregates.

This is a much better candidate for a Domain Service/Policy.

---

# 20. Domain Service vs Domain Policy

The names are not universally standardized, but a useful distinction is:

## Domain Service

Usually represents domain behavior/operation:

```text
Calculate...
Determine...
Validate...
Authorize...
Evaluate...
```

especially when the behavior doesn't naturally belong to one object.

## Domain Policy

Usually represents a business rule or decision:

```text
Is this allowed?
Can this happen?
Which options are available?
What conditions must be satisfied?
```

Your workflow matrix is naturally a Policy:

```csharp
WorkflowPolicy.IsAllowed(key);
```

and:

```csharp
WorkflowPolicy.GetAllowedActions(status, subStatus);
```

---

# 21. Why the Workflow Policy Is Not Application Logic

Consider:

```text
Draft + SubmitForManagerReview = Allowed
```

This is a business rule.

It doesn't depend on:

```text
HTTP
REST
JSON
Controller
Database
EF Core
```

Therefore it belongs to the Domain.

The API might send:

```http
POST /opportunities/123/submit-for-manager-review
```

but the business doesn't care that the request arrived through HTTP.

The business concept is:

```text
SubmitForManagerReview
```

---

# 22. Simplifying the Workflow Architecture

An earlier design might have been:

```text
API
 |
 v
WorkflowExecutor
 |
 v
WorkflowResolver
 |
 v
WorkflowTransition
 |
 v
Apply()
 |
 v
Opportunity.SubmitForManagerReview()
```

This introduces several layers of indirection.

If the only purpose of the classes is:

```text
SubmitForManagerReviewTransition.Apply()
    -> opportunity.SubmitForManagerReview()
```

then the abstraction isn't providing meaningful behavior.

It is ceremony.

---

# 23. Simpler Design

A better design is:

```text
API
 |
 v
Application Use Case
 |
 v
Opportunity.SubmitForManagerReview()
 |
 v
WorkflowPolicy.IsAllowed()
 |
 v
Perform domain behavior
```

The Application layer does not need a generic Executor or Resolver.

---

# 24. Aggregate Protects Itself

The aggregate should not rely on every caller remembering all business rules.

For example:

```csharp
public WorkflowSubmissionResult SubmitForManagerReview(...)
{
    EnsureWorkflowTransitionAllowed(
        WorkflowAction.SubmitForManagerReview);

    // Business operation...

    Status = OpportunityStatus.PendingManagerReview;

    // Create submission
    // Create audit
    // Update version

    return ...;
}
```

Validation:

```csharp
private void EnsureWorkflowTransitionAllowed(
    WorkflowAction action)
{
    var key = new WorkflowKey(
        Status,
        SubStatus,
        action);

    if (!WorkflowPolicy.IsAllowed(key))
    {
        throw new WorkflowTransitionNotAllowedException(key);
    }
}
```

This means the aggregate remains safe regardless of who calls it.

---

# 25. Why Validate Inside the Aggregate?

Today the caller might be:

```text
REST API
```

Tomorrow it could be:

```text
Message Consumer
Background Job
Another Application Use Case
Batch Process
```

You don't want every caller to remember:

```csharp
if (!WorkflowPolicy.IsAllowed(...))
```

The aggregate should protect its own invariants.

Therefore:

```csharp
opportunity.SubmitForManagerReview();
```

is safe regardless of where the call originated.

---

# 26. Domain Exception

The Domain should not throw an HTTP-specific exception.

Bad:

```csharp
throw new CustomException(
    HttpCustomErrorCode.CustomError,
    ...);
```

The Domain should not know about HTTP.

Instead:

```csharp
public sealed class WorkflowTransitionNotAllowedException
    : DomainException
{
    public WorkflowTransitionNotAllowedException(
        WorkflowKey key)
        : base(
            $"Workflow action '{key.Action}' is not allowed " +
            $"from status '{key.Status}' " +
            $"and sub-status '{key.SubStatus}'.")
    {
        Key = key;
    }

    public WorkflowKey Key { get; }
}
```

Then the Application/API infrastructure can map the Domain exception to the appropriate HTTP response.

---

# 27. Domain Result vs Application Result

Avoid making Domain results depend on API/application types.

A Domain method can return:

```csharp
public sealed record WorkflowSubmissionResult(
    OpportunityVersion Version,
    OpportunitySubmission Submission,
    OpportunityAuditHistory Audit);
```

That is a Domain result because it contains Domain objects.

Do not make the Domain return:

```csharp
HttpResponse
ApiResult
CustomException
ControllerResult
```

The Application layer can map Domain results to application DTOs.

---

# 28. Example: Complete Domain Model

A simplified model:

```text
Domain
|
+-- Opportunity Aggregate
|     |
|     +-- Opportunity              <- Aggregate Root
|     +-- OpportunityVersion
|     +-- OpportunitySubmission
|     +-- OpportunityAuditHistory
|
+-- Workflow
|     |
|     +-- WorkflowAction            <- Enum
|     +-- WorkflowKey               <- Value Object
|     +-- WorkflowPolicy             <- Domain Policy
|     +-- WorkflowSubmissionResult
|     +-- WorkflowTransitionNotAllowedException
|
+-- Other Domain Policies/Services
```

---

# 29. Example: Complete Submission Flow

The API receives:

```text
SubmitForManagerReview
```

Application:

```csharp
var opportunity =
    await repository.GetByIdAsync(id, cancellationToken);

var result =
    opportunity.SubmitForManagerReview(
        content,
        userId,
        userName,
        editSummary,
        DateTime.UtcNow);

await unitOfWork.SaveChangesAsync(cancellationToken);
```

Domain:

```text
Opportunity.SubmitForManagerReview()
        |
        v
Create WorkflowKey
        |
        v
WorkflowPolicy.IsAllowed()
        |
    +---+---+
    |       |
   NO      YES
    |       |
    v       v
Exception  Execute
           business rules
```

---

# 30. Getting Available Actions

The workflow policy can also answer:

> What actions are currently available?

```csharp
var actions = WorkflowPolicy.GetAllowedActions(
    opportunity.Status,
    opportunity.SubStatus);
```

For example:

```text
Draft
    -> SubmitForManagerReview

PendingManagerReview
    -> Approve
    -> Reject
    -> RequestModification

Approved
    -> Publish
```

This is useful for APIs such as:

```text
GET /opportunities/{id}/available-actions
```

The Application layer can call the Domain policy and then map the result to an API DTO.

---

# 31. Important: Available Actions Are Not Security Authorization

This distinction matters.

The Domain workflow may say:

```text
Approve = allowed
```

That does not necessarily mean the current user is authorized to approve.

There are two different questions:

```text
Workflow:
Can the opportunity be approved from its current business state?

Authorization:
Is this user allowed to perform approval?
```

Workflow:

```csharp
WorkflowPolicy.IsAllowed(key)
```

Authorization:

```text
Application / Authorization layer
```

The Domain workflow should not become a replacement for your identity/permission system.

---

# 32. The "Who Decides?" Model

A useful way to understand the architecture is to ask:

## API

> How did the request arrive?

Examples:

```text
HTTP
REST
JSON
Route
Headers
```

## Application

> What use case are we executing?

Examples:

```text
Load Opportunity
Invoke SubmitForManagerReview
Save Unit of Work
Return result
```

## Domain Aggregate

> What does it mean to perform this business operation?

Examples:

```text
Submit
Approve
Reject
Publish
```

## Domain Policy

> Is this business operation allowed under the current business rules?

Examples:

```text
Draft + Submit = allowed
Published + Submit = not allowed
```

## Repository / Infrastructure

> How do we store and retrieve it?

---

# 33. Consistency Boundary and Transactions

The aggregate boundary often corresponds closely to the transaction boundary.

For example:

```text
Opportunity Aggregate
|
+-- Opportunity
+-- Current Version
+-- Submission
+-- Audit
```

A command such as:

```csharp
opportunity.SubmitForManagerReview();
```

may change all of these.

The Application layer then persists the aggregate:

```csharp
await unitOfWork.SaveChangesAsync();
```

The expectation is that the aggregate's required state changes are persisted consistently.

This does NOT mean every related object in the database must belong to the same aggregate.

The aggregate boundary is a **business consistency boundary**, not simply a database foreign-key boundary.

---

# 34. Aggregate Boundary Is Not the Same as Database Relationships

You might have:

```text
Opportunity
    |
    +-- SectorId
```

That does not automatically mean:

```text
Sector
```

belongs to the Opportunity aggregate.

An aggregate is determined by business consistency requirements, not by:

```text
FK exists
Navigation property exists
EF relationship exists
```

For example:

```text
Opportunity Aggregate
        |
        +-- SectorId
             |
             v
        Sector Aggregate
```

The opportunity can reference the sector without owning the sector.

---

# 35. Cross-Aggregate Rules

If an operation requires information from another aggregate:

```text
Opportunity
+
Organization
+
Sector
```

avoid putting the entire other aggregate inside the Opportunity aggregate merely to make the code convenient.

Instead, use:

```text
Application orchestration
+
Domain policies/services
```

depending on the exact business rule.

The important principle is:

> An aggregate should be small enough that its invariants can be maintained consistently.

---

# 36. Do Not Create Aggregates Based on Tables

A common mistake is:

```text
Table -> Entity -> Aggregate
```

automatically.

For example:

```text
OpportunityVersion table
    -> OpportunityVersion Aggregate

Submission table
    -> Submission Aggregate

Audit table
    -> Audit Aggregate
```

This may be wrong.

Instead ask:

> What business invariants need to be protected together?

If the answer is:

> The Opportunity root must control its versions and submissions during a workflow operation.

then they can be part of the Opportunity aggregate.

---

# 37. DDD Concepts in the OpportunityHub Domain

| Concept | Example | Purpose |
|---|---|---|
| Entity | `Opportunity` | Has identity |
| Aggregate | Opportunity + versions + submissions | Consistency boundary |
| Aggregate Root | `Opportunity` | Controls aggregate |
| Value Object | `WorkflowKey` | Represents a value |
| Enum | `WorkflowAction` | Business vocabulary |
| Domain Policy | `WorkflowPolicy` | Workflow rules |
| Domain Service | `PublishingPolicy` if cross-aggregate behavior requires it | Domain behavior without natural owner |
| Domain Exception | `WorkflowTransitionNotAllowedException` | Business rule failure |
| Application Use Case | `SubmitForManagerReview` | Orchestrates execution |
| Repository | `IOpportunityRepository` | Loads/saves aggregate |
| API | HTTP endpoint | External interface |

---

# 38. Practical Decision Tree

When you have some business logic, ask:

```text
Does this represent a value?
        |
       YES
        |
   Value Object


Does it have identity?
        |
       YES
        |
      Entity


Does it belong to a consistency boundary?
        |
       YES
        |
     Aggregate


Is it the entity controlling that boundary?
        |
       YES
        |
  Aggregate Root


Does the behavior naturally belong to the
aggregate root/entity/value object?
        |
       YES
        |
 Put behavior there.


Does it represent a reusable business rule/policy?
        |
       YES
        |
 Domain Policy


Is it domain behavior involving concepts that
do not naturally belong to one domain object?
        |
       YES
        |
 Domain Service


Is it only coordinating a use case?
        |
       YES
        |
 Application Layer
```

---

# 39. The Most Important Principles

## Principle 1

Do not move business logic to Application merely because the API invokes it.

```text
API request
    !=
Application-owned business rule
```

The API is just one way of invoking the business.

---

## Principle 2

Do not create a Domain Service just because multiple entities are involved.

If they belong to one aggregate, the Aggregate Root can own the operation.

---

## Principle 3

The Aggregate Root protects its aggregate.

```csharp
opportunity.SubmitForManagerReview();
```

is preferable to allowing callers to manipulate:

```text
OpportunityVersion
Submission
Audit
Status
```

independently.

---

## Principle 4

The Domain should not know about HTTP.

Avoid:

```text
HttpStatusCode
Controller
ApiResponse
HTTP-specific exception
```

inside the Domain.

---

## Principle 5

Workflow transitions are business knowledge.

Therefore:

```csharp
WorkflowPolicy.IsAllowed(...)
```

belongs in the Domain.

---

## Principle 6

Do not create abstractions merely to follow a pattern.

If:

```text
Resolver
    -> Transition
        -> Apply()
            -> Aggregate
```

only forwards a call, it is probably unnecessary.

Prefer:

```text
Application
    -> Aggregate Root
```

with the Aggregate using Domain Policies as needed.

---

# 40. Recommended Architecture for This Domain

```text
OpportunityHub.Domain
|
+-- Entities
|   |
|   +-- Opportunity
|   +-- OpportunityVersion
|   +-- OpportunitySubmission
|   +-- OpportunityAuditHistory
|
+-- Workflow
|   |
|   +-- WorkflowAction.cs
|   +-- WorkflowKey.cs
|   +-- WorkflowPolicy.cs
|   +-- WorkflowSubmissionResult.cs
|   +-- WorkflowTransitionNotAllowedException.cs
|
+-- Exceptions
|
+-- ValueObjects
|
+-- Other Domain Policies
```

Application:

```text
OpportunityHub.Application
|
+-- Opportunities
|   |
|   +-- SubmitForManagerReview
|   +-- Approve
|   +-- Reject
|   +-- Publish
|
+-- Interfaces
|   |
|   +-- IOpportunityRepository
|   +-- IUnitOfWork
|
+-- DTOs
```

Infrastructure:

```text
OpportunityHub.Infrastructure
|
+-- Persistence
|   |
|   +-- EF Core
|   +-- Repositories
|   +-- Configurations
```

API:

```text
OpportunityHub.Api
|
+-- Controllers
+-- HTTP models
+-- Exception mapping
+-- Authentication / authorization integration
```

---

# 41. Final Mental Model

If you remember only one diagram, remember this:

```text
                    BUSINESS DOMAIN
                          |
        +-----------------+------------------+
        |                 |                  |
        v                 v                  v
     Entities          Policies          Value Objects
        |                 |                  |
        +-----------------+------------------+
                          |
                          v
                    Aggregate Root
                          |
                  "Perform this business
                       operation"
                          |
                          v
                  Maintain invariants
                          |
                          v
                    Domain Result


                    APPLICATION
                          |
                  "Execute this use case"
                          |
        +-----------------+------------------+
        |                 |                  |
     Load data       Invoke domain       Persist
        |              behavior              |
        +-----------------+------------------+
                          |
                          v
                         API
```

The central idea is:

> **The Domain models and protects the business. The Application layer coordinates the use case. The API exposes the use case.**

And for the OpportunityHub workflow specifically:

```text
SubmitForManagerReview request
        |
        v
Application
        |
        v
Opportunity.SubmitForManagerReview()
        |
        v
WorkflowPolicy.IsAllowed(
    WorkflowKey(
        current status,
        current sub-status,
        SubmitForManagerReview))
        |
      YES
        |
        v
Execute domain behavior
        |
        +-- Update version
        +-- Change status
        +-- Create submission
        +-- Create audit
        |
        v
Save aggregate
```

This keeps the workflow rules in the Domain, the use-case orchestration in Application, and the actual opportunity behavior inside the Aggregate Root.