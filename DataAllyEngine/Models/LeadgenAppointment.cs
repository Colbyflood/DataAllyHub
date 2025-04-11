using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAllyEngine.Models;

[Table("leadgenappointment")]
[Index("LeadgenkpiId", Name = "LeadGenAppointment_LeadGen_FK")]
[Index("LeadgenkpiId", Name = "leadgenkpi_id_UNIQUE", IsUnique = true)]
public partial class LeadgenAppointment
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("leadgenkpi_id")]
    public int LeadgenkpiId { get; set; }

    [Column("appointments_scheduled")]
    public int? AppointmentsScheduled { get; set; }

    [Column("appointments_scheduled_value")]
    [Precision(10, 4)]
    public decimal? AppointmentsScheduledValue { get; set; }

    [Column("appointments_scheduled_rate")]
    [Precision(10)]
    public decimal? AppointmentsScheduledRate { get; set; }

    [Column("cost_per_appointment_scheduled")]
    [Precision(10, 4)]
    public decimal? CostPerAppointmentScheduled { get; set; }

    [Column("website_appointments_scheduled")]
    public int? WebsiteAppointmentsScheduled { get; set; }

    [Column("website_appointments_scheduled_value")]
    [Precision(10, 4)]
    public decimal? WebsiteAppointmentsScheduledValue { get; set; }

    [Column("cost_per_website_appointments_scheduled")]
    [Precision(10, 4)]
    public decimal? CostPerWebsiteAppointmentsScheduled { get; set; }

    [Column("mobile_app_appointments_scheduled")]
    public int? MobileAppAppointmentsScheduled { get; set; }

    [Column("mobile_app_appointments_scheduled_value")]
    [Precision(10, 4)]
    public decimal? MobileAppAppointmentsScheduledValue { get; set; }

    [Column("cost_per_mobile_app_appointments_scheduled")]
    [Precision(10, 4)]
    public decimal? CostPerMobileAppAppointmentsScheduled { get; set; }

    [Column("offline_appointments_scheduled")]
    public int? OfflineAppointmentsScheduled { get; set; }

    [Column("offline_appointments_scheduled_value")]
    [Precision(10, 4)]
    public decimal? OfflineAppointmentsScheduledValue { get; set; }

    [Column("cost_per_offline_appointments_scheduled")]
    [Precision(10, 4)]
    public decimal? CostPerOfflineAppointmentsScheduled { get; set; }

    [ForeignKey("LeadgenkpiId")]
    public virtual LeadgenKpi LeadgenKpi { get; set; } = null!;
}
