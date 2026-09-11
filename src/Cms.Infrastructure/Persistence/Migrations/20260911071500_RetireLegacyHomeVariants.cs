using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cms.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Classic (1), Modern (2) and Academic (4) were withdrawn, leaving any website that had
    /// chosen one holding a number no design answers to. Nothing in the schema changes; the
    /// stored numbers are moved onto the nearest surviving design so every website keeps a
    /// typeface pairing and a home layout instead of falling through to a default.
    ///
    /// There is no Down: the retired designs no longer exist in the application, so putting
    /// the old numbers back would only recreate the orphaned state.
    /// </summary>
    public partial class RetireLegacyHomeVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Classic -> Prestige, Modern -> Campus, Academic -> Atrium.
            migrationBuilder.Sql("UPDATE [Sites] SET [HomeVariant] = 5 WHERE [HomeVariant] = 1;");
            migrationBuilder.Sql("UPDATE [Sites] SET [HomeVariant] = 3 WHERE [HomeVariant] = 2;");
            migrationBuilder.Sql("UPDATE [Sites] SET [HomeVariant] = 7 WHERE [HomeVariant] = 4;");

            // Anything else outside the surviving set is a stray value; Prestige is the default.
            migrationBuilder.Sql("UPDATE [Sites] SET [HomeVariant] = 5 WHERE [HomeVariant] NOT IN (3, 5, 6, 7);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
