using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EKR_AuthorizeService.Migrations
{
    /// <inheritdoc />
    public partial class IVAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "IV",
                table: "Sessions",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IV",
                table: "Sessions");
        }
    }
}
