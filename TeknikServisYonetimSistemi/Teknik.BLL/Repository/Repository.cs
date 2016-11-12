using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teknik.Entity.Entities;

namespace Teknik.BLL.Repository
{
    public class ArizaRepo : RepositoryBase<Ariza, int> { }
    public class ArizaBilgilendirmeRepo : RepositoryBase<ArizaBilgilendirme, int> { }
    public class FotografRepo : RepositoryBase<Fotograf, int> { }
    public class PcMarkaRepo : RepositoryBase<PcMarka, int> { }
    public class PcModelRepo : RepositoryBase<PcModel, int> { }
    public class AnketRepo : RepositoryBase<Anket, int> { }
}
