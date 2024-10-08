﻿CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "Jobs" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "UserId" uuid NOT NULL,
    "Source" text NOT NULL,
    "SourceId" text NOT NULL,
    "Title" text NOT NULL,
    "Company" text NOT NULL,
    "Location" text NOT NULL,
    "Url" text NOT NULL,
    "PostedDate" timestamp with time zone NOT NULL,
    "EmploymentType" text,
    "LocationType" text,
    "NumberOfEmployees" text,
    "JobDescription" text,
    CONSTRAINT "PK_Jobs" PRIMARY KEY ("Id")
);

CREATE TABLE "ProceedJobCheckpoints" (
    "Id" integer GENERATED BY DEFAULT AS IDENTITY,
    "ServiceName" text NOT NULL,
    "Checkpoint" integer NOT NULL,
    "TimeStamp" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ProceedJobCheckpoints" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_Jobs_SourceId" ON "Jobs" ("SourceId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20240815190537_Initial', '8.0.8');

COMMIT;

START TRANSACTION;

ALTER TABLE "Jobs" ADD "Keywords" jsonb NOT NULL DEFAULT '{}';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20240815191446_AddKeywordsToJobTable', '8.0.8');

COMMIT;

