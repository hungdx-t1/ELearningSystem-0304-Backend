CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TYPE lesson_type AS ENUM ('video', 'document', 'quiz');
CREATE TYPE user_role AS ENUM ('admin', 'instructor', 'student');
CREATE TYPE video_provider AS ENUM ('youtube', 'local_upload');

CREATE TABLE "Classes" (
    "Id" uuid NOT NULL,
    "ClassCode" text NOT NULL,
    "ClassName" text NOT NULL,
    "GoogleMeetLink" text,
    "AcademicYear" text,
    "Description" text,
    CONSTRAINT "PK_Classes" PRIMARY KEY ("Id")
);

CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "UserCode" text NOT NULL,
    "FullName" text NOT NULL,
    "Email" text NOT NULL,
    "PasswordHash" text NOT NULL,
    "Role" integer NOT NULL,
    "AvatarUrl" text,
    "DateOfBirth" timestamp with time zone,
    "AdministrativeClass" text,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE TABLE "AiChatLogs" (
    "Id" uuid NOT NULL,
    "UserId" uuid,
    "Message" text NOT NULL,
    "Response" text NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_AiChatLogs" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AiChatLogs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id")
);

CREATE TABLE "ClassEnrollments" (
    "ClassId" uuid NOT NULL,
    "StudentId" uuid NOT NULL,
    "EnrollmentDate" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ClassEnrollments" PRIMARY KEY ("ClassId", "StudentId"),
    CONSTRAINT "FK_ClassEnrollments_Classes_ClassId" FOREIGN KEY ("ClassId") REFERENCES "Classes" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ClassEnrollments_Users_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Courses" (
    "Id" uuid NOT NULL,
    "Title" text NOT NULL,
    "Description" text,
    "InstructorId" uuid,
    "ThumbnailUrl" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Courses" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Courses_Users_InstructorId" FOREIGN KEY ("InstructorId") REFERENCES "Users" ("Id") ON DELETE SET NULL
);

CREATE TABLE "Chapters" (
    "Id" uuid NOT NULL,
    "CourseId" uuid NOT NULL,
    "Title" text NOT NULL,
    "SortOrder" integer NOT NULL,
    CONSTRAINT "PK_Chapters" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Chapters_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Lessons" (
    "Id" uuid NOT NULL,
    "ChapterId" uuid NOT NULL,
    "Title" text NOT NULL,
    "Type" integer NOT NULL,
    "VideoProvider" integer,
    "VideoUrl" text,
    "DocumentUrl" text,
    "Duration" integer,
    "SortOrder" integer NOT NULL,
    CONSTRAINT "PK_Lessons" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Lessons_Chapters_ChapterId" FOREIGN KEY ("ChapterId") REFERENCES "Chapters" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Assignments" (
    "Id" uuid NOT NULL,
    "LessonId" uuid NOT NULL,
    "Title" text NOT NULL,
    "Description" text,
    "DueDate" timestamp with time zone,
    "MaxScore" real NOT NULL,
    CONSTRAINT "PK_Assignments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Assignments_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES "Lessons" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Submissions" (
    "Id" uuid NOT NULL,
    "AssignmentId" uuid NOT NULL,
    "StudentId" uuid NOT NULL,
    "SubmissionUrl" text,
    "StudentNote" text,
    "SubmittedAt" timestamp with time zone NOT NULL,
    "Score" real,
    "Feedback" text,
    CONSTRAINT "PK_Submissions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Submissions_Assignments_AssignmentId" FOREIGN KEY ("AssignmentId") REFERENCES "Assignments" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Submissions_Users_StudentId" FOREIGN KEY ("StudentId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AiChatLogs_UserId" ON "AiChatLogs" ("UserId");

CREATE INDEX "IX_Assignments_LessonId" ON "Assignments" ("LessonId");

CREATE INDEX "IX_Chapters_CourseId" ON "Chapters" ("CourseId");

CREATE INDEX "IX_ClassEnrollments_StudentId" ON "ClassEnrollments" ("StudentId");

CREATE INDEX "IX_Courses_InstructorId" ON "Courses" ("InstructorId");

CREATE INDEX "IX_Lessons_ChapterId" ON "Lessons" ("ChapterId");

CREATE INDEX "IX_Submissions_AssignmentId" ON "Submissions" ("AssignmentId");

CREATE INDEX "IX_Submissions_StudentId" ON "Submissions" ("StudentId");

CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

CREATE UNIQUE INDEX "IX_Users_UserCode" ON "Users" ("UserCode");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260303040940_InitialCreate', '10.0.3');

COMMIT;

START TRANSACTION;
ALTER TABLE "Courses" DROP CONSTRAINT "FK_Courses_Users_InstructorId";

DROP INDEX "IX_Courses_InstructorId";

ALTER TABLE "Courses" DROP COLUMN "InstructorId";

ALTER TABLE "Classes" ADD "CourseId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE "Classes" ADD "InstructorId" uuid;

CREATE INDEX "IX_Classes_CourseId" ON "Classes" ("CourseId");

CREATE INDEX "IX_Classes_InstructorId" ON "Classes" ("InstructorId");

ALTER TABLE "Classes" ADD CONSTRAINT "FK_Classes_Courses_CourseId" FOREIGN KEY ("CourseId") REFERENCES "Courses" ("Id") ON DELETE CASCADE;

ALTER TABLE "Classes" ADD CONSTRAINT "FK_Classes_Users_InstructorId" FOREIGN KEY ("InstructorId") REFERENCES "Users" ("Id") ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260320051454_MoveInstructorToClass', '10.0.3');

COMMIT;

START TRANSACTION;
CREATE TABLE "Questions" (
    "Id" uuid NOT NULL,
    "LessonId" uuid NOT NULL,
    "Content" text NOT NULL,
    "OptionA" text NOT NULL,
    "OptionB" text NOT NULL,
    "OptionC" text NOT NULL,
    "OptionD" text NOT NULL,
    "CorrectOption" text NOT NULL,
    "Explanation" text,
    CONSTRAINT "PK_Questions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Questions_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES "Lessons" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Questions_LessonId" ON "Questions" ("LessonId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260323095608_AddQuestionTable', '10.0.3');

COMMIT;

START TRANSACTION;
ALTER TABLE "Submissions" DROP CONSTRAINT "FK_Submissions_Assignments_AssignmentId";

ALTER TABLE "Submissions" RENAME COLUMN "AssignmentId" TO "LessonId";

ALTER INDEX "IX_Submissions_AssignmentId" RENAME TO "IX_Submissions_LessonId";

ALTER TYPE lesson_type ADD VALUE 'assignment';

ALTER TABLE "Submissions" ADD "ClassId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

CREATE INDEX "IX_Submissions_ClassId" ON "Submissions" ("ClassId");

ALTER TABLE "Submissions" ADD CONSTRAINT "FK_Submissions_Classes_ClassId" FOREIGN KEY ("ClassId") REFERENCES "Classes" ("Id") ON DELETE CASCADE;

ALTER TABLE "Submissions" ADD CONSTRAINT "FK_Submissions_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES "Lessons" ("Id") ON DELETE CASCADE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260324020227_UpdateSubmissionAndLessonType', '10.0.3');

COMMIT;

START TRANSACTION;
ALTER TABLE "Courses" ADD "CreatorId" uuid;

CREATE INDEX "IX_Courses_CreatorId" ON "Courses" ("CreatorId");

ALTER TABLE "Courses" ADD CONSTRAINT "FK_Courses_Users_CreatorId" FOREIGN KEY ("CreatorId") REFERENCES "Users" ("Id");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260407081732_AddCreatorToCourse', '10.0.3');

COMMIT;

START TRANSACTION;
ALTER TABLE "Courses" ADD "IsPublic" boolean NOT NULL DEFAULT FALSE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260408020719_AddIsPublicToCourse', '10.0.3');

COMMIT;

START TRANSACTION;
ALTER TABLE "Submissions" ADD "CheatWarnings" integer NOT NULL DEFAULT 0;

ALTER TABLE "Submissions" ADD "IsSubmitted" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "Submissions" ADD "QuizAnswersJson" text;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260408101235_AddAntiCheatToSubmission', '10.0.3');

COMMIT;

START TRANSACTION;
ALTER TABLE "Lessons" ADD "IsExam" boolean NOT NULL DEFAULT FALSE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260409023030_AddIsExamInLesson', '10.0.3');

COMMIT;

START TRANSACTION;
ALTER TABLE "Submissions" ADD "StartedAt" timestamp with time zone;

CREATE TABLE "ClassLessonSchedules" (
    "ClassId" uuid NOT NULL,
    "LessonId" uuid NOT NULL,
    "StartTime" timestamp with time zone,
    "DueDate" timestamp with time zone,
    "OverrideDuration" integer,
    CONSTRAINT "PK_ClassLessonSchedules" PRIMARY KEY ("ClassId", "LessonId"),
    CONSTRAINT "FK_ClassLessonSchedules_Classes_ClassId" FOREIGN KEY ("ClassId") REFERENCES "Classes" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ClassLessonSchedules_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES "Lessons" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_ClassLessonSchedules_LessonId" ON "ClassLessonSchedules" ("LessonId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260420032802_AddClassLessonSchedule', '10.0.3');

COMMIT;

START TRANSACTION;
ALTER TABLE "Users" ADD "OtpCode" text;

ALTER TABLE "Users" ADD "OtpExpiryTime" timestamp with time zone;

ALTER TABLE "Users" ADD "PendingNewEmail" text;

ALTER TABLE "Users" ADD "ResetToken" text;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260421075855_AddOtpToUser', '10.0.3');

COMMIT;

