using System;

namespace YKnytt.Services
{
    public class LevelMetadata
    {
        public string LevelId { get; set; }    
        public string Name { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }

        public float StarRating { get; set; }
        public int RatingCount { get; set; }

        public string State { get; set; }

        public DateTime LastUpdated { get; set; }
        // convenience constructor 
        public LevelMetadata() { }
        public LevelMetadata(
            string levelId,
            string name,
            string author,
            string category,
            float starRating,
            int ratingCount,
            string state,
            DateTime lastUpdated)
        {
            LevelId = levelId;
            Name = name;
            Author = author;
            Category = category;
            StarRating = starRating;
            RatingCount = ratingCount;
            State = state;
            LastUpdated = lastUpdated;
        }
        // lightweight sanity check (not validation framework level)
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(LevelId);
        }
        public override string ToString()
        {
            return $"{LevelId} | {Name} ({Author}) ★{StarRating} [{State}]";
        }
    }
}
