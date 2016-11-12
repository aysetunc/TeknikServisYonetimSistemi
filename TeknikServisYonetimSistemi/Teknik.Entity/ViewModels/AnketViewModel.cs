using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Entity.ViewModels
{
    public class AnketViewModel
    {
        
        public int ID { get; set; }
        [Display(Name = "Arıza numarası")]
        public int ArizaID { get; set; }
        [Display(Name = "Teknisyeni")]
        public string TeknikerID { get; set; }
        [Display(Name = "Anketi Gönderen")]
        public string KullaniciID { get; set; }
        [Display(Name = "Arıza Memnuniyet Puanı")]
        public string Puan { get; set; }
        [Display(Name = "Düşünceleriniz")]
        public string Aciklama { get; set; }
    }
}
