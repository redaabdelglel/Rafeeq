
using AutoMapper;
using Rafeeq.DTOs.Availability;
using Rafeeq.DTOs.Users;
using Rafeeq.Models;
using System;

namespace Rafeeq.Mapping
{
    public class MentorProfile : Profile
    {
        public MentorProfile()
        {
            CreateMap<User, MentorDto>()
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src =>
                    (src.MentorSkills != null)
                        ? src.MentorSkills.Where(ms => ms.Skill != null).Select(ms => ms.Skill.Name).ToList()
                        : new List<string>()))
                .ForMember(dest => dest.Availabilities, opt => opt.MapFrom(src =>
                    (src.Availabilities != null)
                        ? src.Availabilities.Select(a => new AvailabilityDto
                        {
                            AvailabilityId = a.AvailabilityId,
                            StartTime = a.StartTime ?? TimeSpan.Zero,
                            EndTime = a.EndTime ?? TimeSpan.Zero
                        }).ToList()
                        : new List<AvailabilityDto>()));
        }
    }
}