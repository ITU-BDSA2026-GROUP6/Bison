using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bison.Razor.Migrations;

public partial class UnifyAuthorIdentity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Rebuild Posts without cascading deletes into observations, comments or proposals.
        migrationBuilder.Sql("PRAGMA foreign_keys = OFF;", suppressTransaction: true);
        migrationBuilder.Sql("""
            CREATE TABLE "AuthorAccountMap" (
                "AuthorId" INTEGER NOT NULL PRIMARY KEY,
                "UserId" TEXT NOT NULL UNIQUE
            );
            INSERT INTO "AuthorAccountMap" ("AuthorId", "UserId")
            SELECT "AuthorId", COALESCE("BisonUserId", 'legacy-author-' || "AuthorId")
            FROM "Authors";

            -- Unlinked authors remain distinct, passwordless accounts. Never infer ownership
            -- from a matching email or display name.
            INSERT INTO "AspNetUsers" (
                "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
                "EmailConfirmed", "PhoneNumberConfirmed", "TwoFactorEnabled",
                "LockoutEnabled", "AccessFailedCount", "SecurityStamp", "ConcurrencyStamp")
            SELECT m."UserId",
                CASE WHEN trim(a."Name") = '' OR EXISTS (
                    SELECT 1 FROM "AspNetUsers" u WHERE u."NormalizedUserName" = upper(a."Name")
                ) OR EXISTS (
                    SELECT 1 FROM "Authors" other
                    WHERE other."AuthorId" <> a."AuthorId" AND upper(other."Name") = upper(a."Name")
                ) THEN 'legacy-' || lower(hex(randomblob(16))) ELSE a."Name" END,
                NULL, NULLIF(a."Email", ''), NULL,
                0, 0, 0, 1, 0, lower(hex(randomblob(16))), lower(hex(randomblob(16)))
            FROM "Authors" a JOIN "AuthorAccountMap" m ON m."AuthorId" = a."AuthorId"
            WHERE a."BisonUserId" IS NULL;

            -- Keep legacy email addresses for attribution, but do not make unverified
            -- legacy records discoverable by Identity email lookup/password reset.
            UPDATE "AspNetUsers" SET "NormalizedUserName" = upper("UserName")
            WHERE "Id" IN (
                SELECT m."UserId" FROM "AuthorAccountMap" m
                JOIN "Authors" a ON a."AuthorId" = m."AuthorId"
                WHERE a."BisonUserId" IS NULL
            );

            CREATE TABLE "Posts_Unified" (
                "PostId" INTEGER NOT NULL CONSTRAINT "PK_Posts" PRIMARY KEY AUTOINCREMENT,
                "AuthorId" TEXT NOT NULL,
                "Text" TEXT NOT NULL,
                "TimeStamp" TEXT NOT NULL,
                CONSTRAINT "FK_Posts_AspNetUsers_AuthorId" FOREIGN KEY ("AuthorId")
                    REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );
            INSERT INTO "Posts_Unified" ("PostId", "AuthorId", "Text", "TimeStamp")
            SELECT p."PostId", (SELECT m."UserId" FROM "AuthorAccountMap" m
                WHERE m."AuthorId" = p."AuthorId"), p."Text", p."TimeStamp"
            FROM "Posts" p;
            DROP TABLE "Posts";
            ALTER TABLE "Posts_Unified" RENAME TO "Posts";
            CREATE INDEX "IX_Posts_AuthorId" ON "Posts" ("AuthorId");
            DROP TABLE "Authors";
            DROP TABLE "AuthorAccountMap";
            -- Fail within the transaction rather than commit broken ownership references.
            CREATE TABLE "IdentityMigrationCheck" ("Violations" INTEGER CHECK ("Violations" = 0));
            INSERT INTO "IdentityMigrationCheck" SELECT count(*) FROM pragma_foreign_key_check;
            DROP TABLE "IdentityMigrationCheck";
            """);
        migrationBuilder.Sql("PRAGMA foreign_keys = ON;", suppressTransaction: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "Unifying accounts cannot restore the original author identities. Restore a database backup to roll back.");
    }
}
