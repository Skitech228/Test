using Microsoft.AspNetCore.Identity;

namespace ANG24.Sys.Domain.DBModels
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public string SecondName { get; set; }
        public string ThirdName { get; set; }
        public string Position { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<ModuleStage> ModuleStages { get; set; } = new List<ModuleStage>();
        public override string ToString() => $"type: User [Id = {Id}, Name = {Name}]";
    }
}
