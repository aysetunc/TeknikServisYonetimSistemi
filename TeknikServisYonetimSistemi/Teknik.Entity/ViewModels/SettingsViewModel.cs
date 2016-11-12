using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Entity.ViewModels
{
    public class SettingsViewModel
    {
        public List<PcMarkaViewModel> PcMarkalari = new List<PcMarkaViewModel>();
        public List<PcModelViewModel> PcModelleri = new List<PcModelViewModel>();
        public List<KullaniciViewModel> Kullanicilar = new List<KullaniciViewModel>();
        public List<YönetimYetkiViewModel> Yetkiler = new List<YönetimYetkiViewModel>();
        public PcModelViewModel PcModeli { get; set; }
        public PcMarkaViewModel PcMarkasi { get; set; }
        public string RoleId { get; set; }
    }
    public class PcMarkaViewModel
    {
        public int ID { get; set; }
        [StringLength(70)]
        [Required]
        [Display(Name = "Marka Adı")]
        public string MarkaAdi { get; set; }
    }
    public class PcModelViewModel
    {
        public int ID { get; set; }
        public int MarkaId { get; set; }
        [Required]
        [Display(Name = "Model Adı")]
        public string ModelAdi { get; set; }
    }
    public class KullaniciViewModel
    {
        public string ID { get; set; }
        [Display(Name = "E-mail")]
        public string Email { get; set; }
        [Display(Name = "Kullanıcı Adı")]
        public string KullaniciAdi { get; set; }
        [Display(Name = "Kullanıcı Rolü")]
        public string Rol { get; set; }
    }
    public class YönetimYetkiViewModel
    {
        public string  Yetki { get; set; }
    }
}
