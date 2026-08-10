using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cms.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260803120000_AddActivityLogs")]
public sealed class AddActivityLogs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ActivityLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ActorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                Action = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                EntityType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ChangedProperties = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_ActivityLogs", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_ActivityLogs_TenantId_SiteId_CreatedDate",
            table: "ActivityLogs",
            columns: new[] { "TenantId", "SiteId", "CreatedDate" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropTable(name: "ActivityLogs");
}
