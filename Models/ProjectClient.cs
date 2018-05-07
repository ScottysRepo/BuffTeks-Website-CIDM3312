using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BuffteksWebsite.Models
{
    public class ProjectClient
    {
        public string ProjectClientID { get; set; }
    public ProjectClient Project_Client { get; set; }
        public string ProjectID { get; set; }
        public Project Project { get; set; }
    }
}