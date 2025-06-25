using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebBaiGiang.Migrations
{
    /// <inheritdoc />
    public partial class addFieldInfos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SocialLink",
                table: "ThongTinWeb");

            migrationBuilder.AddColumn<string>(
                name: "FacebookLink",
                table: "ThongTinWeb",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "YoutubeLink",
                table: "ThongTinWeb",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ZaloLink",
                table: "ThongTinWeb",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacebookLink",
                table: "ThongTinWeb");

            migrationBuilder.DropColumn(
                name: "YoutubeLink",
                table: "ThongTinWeb");

            migrationBuilder.DropColumn(
                name: "ZaloLink",
                table: "ThongTinWeb");

            migrationBuilder.AddColumn<string>(
                name: "SocialLink",
                table: "ThongTinWeb",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }
    }
}
