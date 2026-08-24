FROM liquibase/liquibase:5.0.2

COPY mssql-jdbc.jar /liquibase/lib/mssql-jdbc.jar

RUN test -f /liquibase/lib/mssql-jdbc.jar

WORKDIR /liquibase/changelog
