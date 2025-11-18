using CareerHub.Api.Data;
using CareerHub.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CareerHub.Api.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.10");

            modelBuilder.Entity("CareerHub.Api.Models.Team", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<int>("ManagerId")
                    .HasColumnType("INTEGER");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("ManagerId");

                b.ToTable("Teams");
            });

            modelBuilder.Entity("CareerHub.Api.Models.UserAccount", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<string>("CareerGoal")
                    .HasColumnType("TEXT");

                b.Property<string>("Email")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<string>("PasswordHash")
                    .IsRequired()
                    .HasColumnType("TEXT");

                b.Property<int>("Role")
                    .HasColumnType("INTEGER");

                b.Property<int?>("TeamId")
                    .HasColumnType("INTEGER");

                b.HasKey("Id");

                b.HasIndex("Email")
                    .IsUnique();

                b.HasIndex("TeamId");

                b.ToTable("Users");
            });

            modelBuilder.Entity("CareerHub.Api.Models.Team", b =>
            {
                b.HasOne("CareerHub.Api.Models.UserAccount", "Manager")
                    .WithMany("ManagedTeams")
                    .HasForeignKey("ManagerId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("Manager");
            });

            modelBuilder.Entity("CareerHub.Api.Models.UserAccount", b =>
            {
                b.HasOne("CareerHub.Api.Models.Team", "Team")
                    .WithMany("Employees")
                    .HasForeignKey("TeamId")
                    .OnDelete(DeleteBehavior.SetNull);

                b.Navigation("Team");
            });

            modelBuilder.Entity("CareerHub.Api.Models.Team", b => b.Navigation("Employees"));

            modelBuilder.Entity("CareerHub.Api.Models.UserAccount", b => b.Navigation("ManagedTeams"));
#pragma warning restore 612, 618
        }
    }
}
