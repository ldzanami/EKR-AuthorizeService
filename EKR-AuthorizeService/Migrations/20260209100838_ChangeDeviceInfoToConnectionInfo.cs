using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EKR_AuthorizeService.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDeviceInfoToConnectionInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeviceInfo",
                table: "Sessions",
                newName: "ConnectionInfo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConnectionInfo",
                table: "Sessions",
                newName: "DeviceInfo");
        }
    }
}
