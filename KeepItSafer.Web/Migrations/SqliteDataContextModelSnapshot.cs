using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using KeepItSafer.Web.Data;

namespace KeepItSafer.Web.Migrations
{
    [DbContext(typeof(SqliteDataContext))]
    partial class SqliteDataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("KeepItSafer.Models.UserAccount", b =>
                {
                    b.Property<int>("UserAccountId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AuthenticationUri");

                    b.HasKey("UserAccountId");

                    b.ToTable("UserAccounts");
                });
        }
    }
}
