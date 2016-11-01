namespace Teknik.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class a1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ArizaBilgilendirmeler",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Aciklama = c.String(),
                        AciklamaZamani = c.DateTime(nullable: false, storeType: "smalldatetime"),
                        OlumluMu = c.Boolean(nullable: false),
                        YoneticiID = c.String(maxLength: 128),
                        ArizaID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Arizalar", t => t.ArizaID, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.YoneticiID)
                .Index(t => t.YoneticiID)
                .Index(t => t.ArizaID);
            
            CreateTable(
                "dbo.Arizalar",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Baslik = c.String(maxLength: 70),
                        Aciklama = c.String(),
                        Enlem = c.String(),
                        Boylam = c.String(),
                        Adres = c.String(),
                        EklemeTarihi = c.DateTime(nullable: false, storeType: "smalldatetime"),
                        OnaylamaTarihi = c.DateTime(storeType: "smalldatetime"),
                        OnaylandiMi = c.Boolean(nullable: false),
                        KullaniciID = c.String(maxLength: 128),
                        MarkaID = c.Int(nullable: false),
                        ModelID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.AspNetUsers", t => t.KullaniciID)
                .ForeignKey("dbo.Markalar", t => t.MarkaID, cascadeDelete: true)
                .ForeignKey("dbo.Modeller", t => t.ModelID, cascadeDelete: true)
                .Index(t => t.KullaniciID)
                .Index(t => t.MarkaID)
                .Index(t => t.ModelID);
            
            CreateTable(
                "dbo.Fotograflar",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Yol = c.String(),
                        ArizaID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Arizalar", t => t.ArizaID, cascadeDelete: true)
                .Index(t => t.ArizaID);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(maxLength: 25),
                        Surname = c.String(maxLength: 35),
                        RegisterDate = c.DateTime(nullable: false, storeType: "smalldatetime"),
                        AvatarPath = c.String(),
                        ActivationCode = c.String(),
                        PreRole = c.String(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Markalar",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        MarkaAdi = c.String(maxLength: 70),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Modeller",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ModelAdi = c.String(),
                        MarkaID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Markalar", t => t.MarkaID, cascadeDelete: true)
                .Index(t => t.MarkaID);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                        Description = c.String(maxLength: 200),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Arizalar", "ModelID", "dbo.Modeller");
            DropForeignKey("dbo.Arizalar", "MarkaID", "dbo.Markalar");
            DropForeignKey("dbo.Modeller", "MarkaID", "dbo.Markalar");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ArizaBilgilendirmeler", "YoneticiID", "dbo.AspNetUsers");
            DropForeignKey("dbo.Arizalar", "KullaniciID", "dbo.AspNetUsers");
            DropForeignKey("dbo.Fotograflar", "ArizaID", "dbo.Arizalar");
            DropForeignKey("dbo.ArizaBilgilendirmeler", "ArizaID", "dbo.Arizalar");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Modeller", new[] { "MarkaID" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Fotograflar", new[] { "ArizaID" });
            DropIndex("dbo.Arizalar", new[] { "ModelID" });
            DropIndex("dbo.Arizalar", new[] { "MarkaID" });
            DropIndex("dbo.Arizalar", new[] { "KullaniciID" });
            DropIndex("dbo.ArizaBilgilendirmeler", new[] { "ArizaID" });
            DropIndex("dbo.ArizaBilgilendirmeler", new[] { "YoneticiID" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Modeller");
            DropTable("dbo.Markalar");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Fotograflar");
            DropTable("dbo.Arizalar");
            DropTable("dbo.ArizaBilgilendirmeler");
        }
    }
}
