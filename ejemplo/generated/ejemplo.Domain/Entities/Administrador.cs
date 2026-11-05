using System.Collections.Generic;

namespace ejemplo.Domain.Entities;

public partial class Administrador : Usuario
{
    public string CodigoAuditor { get; set; } = string.Empty;
    public IList<RegistroAuditoria> RegistrosAuditoria { get; set; } = new List<RegistroAuditoria>();
}
