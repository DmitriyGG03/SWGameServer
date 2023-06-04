using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Runtime.Serialization;
using SharedLibrary.Models.Enums;

namespace SharedLibrary.Models
{
    [Table("Planets"), Serializable]
    public class Planet
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public Guid Id { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        [Range(0, int.MaxValue)]
        public int Size { get; set; }

        public PlanetType PlanetType { get; set; }
        public ColorStatus ColorStatus { get; set; } = ColorStatus.Undefined;

        public int Health { get; set; }
        public int HealthLimit { get; set; }

        public bool IsCapital { get; set; } = false;

        public string PlanetName { get; set; }
        public Guid? OwnerId { get; set; }

        public ResourceType ResourceType { get; set; }
        public int ResourceCount { get; set; }

        [NotMapped, IgnoreDataMember]
        public PointF Position
        {
            get => new PointF(X, Y);
        }
        [NotMapped] 
        public bool IsEnemy { get; set; }
        [NotMapped] 
        public PlanetStatus Status { get; set; }
        [NotMapped] 
        public int IterationsLeftToNextStatus { get; set; }
        [NotMapped] 
        public Fortification FortificationLevel { get; set; }

        public Planet()
        {
            Id = Guid.Empty;
            X = Y = 0;
            Size = new Random().Next(1, 1000);
            PlanetName = String.Empty;
        }
        public Planet(PointF position)
        {
            Id = Guid.NewGuid();
            X = position.X;
            Y = position.Y;
            Size = new Random().Next(1, 26);
            PlanetName = String.Empty;
        }
        public Planet(PointF position, int size, string planetName, PlanetType type, 
            ResourceType resourceType, 
            int resourceCount,
            int health)
        {
            Id = Guid.NewGuid();
            X = position.X;
            Y = position.Y;
            Size = size;
            PlanetName = planetName;
            PlanetType = type;
            ResourceType = resourceType;
            ResourceCount = resourceCount;
            Health = HealthLimit = health;

            if (resourceType == ResourceType.ColonizationShip)
            {
                ResourceCount = 1;
            }
            else if (resourceType == ResourceType.ResearchShip)
            {
                ResourceCount = 1;
            }
        }

        public static int CalculateHealthLimit(int planetSize)
        {
            if (planetSize <= 5)
            {
                return planetSize * 15;
            }
            else if (planetSize <= 10)
            {
                return planetSize * 12;
            }
            else if (planetSize <= 20)
            {
                return planetSize * 16;
            }
            else
            {
                return planetSize * 17;
            }
        }

        public int CalculateHealOnTheNextTurn()
        {
            return (int)(HealthLimit * 0.2);
        }
    }
}
