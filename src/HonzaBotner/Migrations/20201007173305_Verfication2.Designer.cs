﻿// <auto-generated />
using HonzaBotner.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HonzaBotner.Migrations
{
    [DbContext(typeof(HonzaBotnerDbContext))]
    [Migration("20201007173305_Verfication2")]
    partial class Verfication2
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.0-rc.1.20451.13");

            modelBuilder.Entity("HonzaBotner.Database.Counter", b =>
                {
                    b.Property<decimal>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Count")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("UserId");

                    b.ToTable("Counters");
                });

            modelBuilder.Entity("HonzaBotner.Database.Verification", b =>
                {
                    b.Property<decimal>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("AuthId")
                        .HasColumnType("text");

                    b.HasKey("UserId");

                    b.ToTable("Verifications");
                });
#pragma warning restore 612, 618
        }
    }
}
