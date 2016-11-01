using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teknik.Entity.Entities
{
    [Table("Markalar")]
    public class PcMarka
    {
        [Key]
        public int ID { get; set; }
        [StringLength(70)]
        public string MarkaAdi { get; set; }

        public virtual List<PcModel> Modelleri { get; set; } = new List<PcModel>();
    }
}
