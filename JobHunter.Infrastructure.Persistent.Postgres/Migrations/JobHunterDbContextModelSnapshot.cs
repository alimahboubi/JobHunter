﻿// <auto-generated />
using System;
using System.Collections.Generic;
using JobHunter.Infrastructure.Persistent.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace JobHunter.Infrastructure.Persistent.Postgres.Migrations
{
    [DbContext(typeof(JobHunterDbContext))]
    partial class JobHunterDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("JobHunter.Domain.Job.Entities.Job", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Company")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("EmploymentType")
                        .HasColumnType("text");

                    b.Property<bool>("IsApplied")
                        .HasColumnType("boolean");

                    b.Property<string>("JobDescription")
                        .HasColumnType("text");

                    b.Property<string>("Keywords")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LocationType")
                        .HasColumnType("text");

                    b.Property<float?>("MatchAccuracy")
                        .HasColumnType("real");

                    b.Property<string>("NumberOfEmployees")
                        .HasColumnType("text");

                    b.Property<DateTime>("PostedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("SourceId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("SourceId")
                        .IsUnique();

                    b.ToTable("Jobs");
                });

            modelBuilder.Entity("JobHunter.Domain.Job.Entities.ProceedJobCheckpoint", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Checkpoint")
                        .HasColumnType("integer");

                    b.Property<string>("ServiceName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("ProceedJobCheckpoints");
                });

            modelBuilder.Entity("JobHunter.Domain.User.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreationDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("LastModificationDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Resume")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("JobHunter.Domain.User.Entities.User", b =>
                {
                    b.OwnsOne("JobHunter.Domain.User.Entities.UserJobTarget", "JobTarget", b1 =>
                        {
                            b1.Property<Guid>("UserId")
                                .HasColumnType("uuid");

                            b1.Property<List<string>>("EssentialKeywords")
                                .IsRequired()
                                .HasColumnType("text[]");

                            b1.Property<string>("JobCategory")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("JobTitle")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<List<string>>("TargetKeywords")
                                .IsRequired()
                                .HasColumnType("text[]");

                            b1.Property<List<string>>("TargetLocations")
                                .IsRequired()
                                .HasColumnType("text[]");

                            b1.HasKey("UserId");

                            b1.ToTable("Users");

                            b1.ToJson("JobTarget");

                            b1.WithOwner()
                                .HasForeignKey("UserId");
                        });

                    b.Navigation("JobTarget")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
