using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProctoringSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProctoringConsentText",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProctoringRetentionDays",
                table: "Tenants",
                type: "integer",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<bool>(
                name: "ProctoringEnabled",
                table: "Exams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireCamera",
                table: "Exams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SnapshotIntervalSeconds",
                table: "Exams",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViolationWarningThreshold",
                table: "Exams",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProctoringConsentOn",
                table: "ExamAttempts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresReview",
                table: "ExamAttempts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReviewApproved",
                table: "ExamAttempts",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                table: "ExamAttempts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedBy",
                table: "ExamAttempts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedOn",
                table: "ExamAttempts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViolationCount",
                table: "ExamAttempts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProctoringEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamAttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    OccurredOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProctoringEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProctoringEvents_ExamAttempts_ExamAttemptId",
                        column: x => x.ExamAttemptId,
                        principalTable: "ExamAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProctoringSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamAttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    CapturedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProctoringSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProctoringSnapshots_ExamAttempts_ExamAttemptId",
                        column: x => x.ExamAttemptId,
                        principalTable: "ExamAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProctoringEvents_ExamAttemptId_OccurredOn",
                table: "ProctoringEvents",
                columns: new[] { "ExamAttemptId", "OccurredOn" });

            migrationBuilder.CreateIndex(
                name: "IX_ProctoringSnapshots_ExamAttemptId_CapturedOn",
                table: "ProctoringSnapshots",
                columns: new[] { "ExamAttemptId", "CapturedOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProctoringEvents");

            migrationBuilder.DropTable(
                name: "ProctoringSnapshots");

            migrationBuilder.DropColumn(
                name: "ProctoringConsentText",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ProctoringRetentionDays",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "ProctoringEnabled",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "RequireCamera",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "SnapshotIntervalSeconds",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "ViolationWarningThreshold",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "ProctoringConsentOn",
                table: "ExamAttempts");

            migrationBuilder.DropColumn(
                name: "RequiresReview",
                table: "ExamAttempts");

            migrationBuilder.DropColumn(
                name: "ReviewApproved",
                table: "ExamAttempts");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                table: "ExamAttempts");

            migrationBuilder.DropColumn(
                name: "ReviewedBy",
                table: "ExamAttempts");

            migrationBuilder.DropColumn(
                name: "ReviewedOn",
                table: "ExamAttempts");

            migrationBuilder.DropColumn(
                name: "ViolationCount",
                table: "ExamAttempts");
        }
    }
}
