﻿// <auto-generated />
using System;
using HonzaBotner.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HonzaBotner.Migrations
{
    [DbContext(typeof(HonzaBotnerDbContext))]
    [Migration("20220521212254_StandUpStats")]
    partial class StandUpStats
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("HonzaBotner.Database.CountedEmoji", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<DateTime>("FirstUsedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("Times")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("CountedEmojis");
                });

            modelBuilder.Entity("HonzaBotner.Database.Reminder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Content")
                        .HasColumnType("text");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("MessageId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("OwnerId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.HasIndex("DateTime");

                    b.ToTable("Reminders");
                });

            modelBuilder.Entity("HonzaBotner.Database.RoleBinding", b =>
                {
                    b.Property<string>("Emoji")
                        .HasColumnType("text");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("MessageId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("RoleId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Emoji", "ChannelId", "MessageId", "RoleId");

                    b.ToTable("RoleBindings");
                });

            modelBuilder.Entity("HonzaBotner.Database.StandUpStat", b =>
                {
                    b.Property<decimal>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("Freezes")
                        .HasColumnType("integer");

                    b.Property<int>("LastDayCompleted")
                        .HasColumnType("integer");

                    b.Property<DateTime>("LastDayOfStreak")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("LastDayTasks")
                        .HasColumnType("integer");

                    b.Property<int>("LongestStreak")
                        .HasColumnType("integer");

                    b.Property<int>("Streak")
                        .HasColumnType("integer");

                    b.Property<int>("TotalCompleted")
                        .HasColumnType("integer");

                    b.Property<int>("TotalTasks")
                        .HasColumnType("integer");

                    b.HasKey("UserId");

                    b.ToTable("StandUpStats");
                });

            modelBuilder.Entity("HonzaBotner.Database.Verification", b =>
                {
                    b.Property<decimal>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("AuthId")
                        .HasColumnType("text");

                    b.HasKey("UserId");

                    b.HasIndex("AuthId")
                        .IsUnique();

                    b.ToTable("Verifications");
                });

            modelBuilder.Entity("HonzaBotner.Database.Warning", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("IssuedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("IssuerId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Reason")
                        .HasColumnType("text");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("Warnings");
                });
#pragma warning restore 612, 618
        }
    }
}
