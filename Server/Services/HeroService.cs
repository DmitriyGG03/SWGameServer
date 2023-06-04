using Microsoft.EntityFrameworkCore;
using Server.Common.Constants;
using Server.Domain;
using Server.Services.Abstract;
using SharedLibrary.Models;

namespace Server.Services;

public interface IHeroService
{
    Task<ServiceResult<Hero>> Update(Guid id, Hero destination, CancellationToken cancellationToken);
    Task<ServiceResult<Hero>> Create(Guid userId, Hero hero, CancellationToken cancellationToken);
    Task<Hero?> GetByIdAsync(Guid heroId, CancellationToken cancellationToken);
    Task<Hero?> GetHeroByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}

public class HeroService : IHeroService
{
    private readonly GameDbContext _dbContext;
    public HeroService(GameDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<Hero>> Update(Guid userId, Hero destination, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.Include(x => x.Heroes).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new ArgumentException(ErrorMessages.User.NotFound);

        var hero = user.Heroes.FirstOrDefault(x => x.HeroId == destination.HeroId);
        if (hero is null)
            return new ServiceResult<Hero>(ErrorMessages.User.HasNoAccess);

        hero.Name = destination.Name;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new ServiceResult<Hero>(hero);
    }

    public async Task<ServiceResult<Hero>> Create(Guid userId, Hero hero, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        
        if (user is null)
            return new ServiceResult<Hero>(ErrorMessages.User.NotFound);

        if (hero.User is null)
            hero.User = user;

        _dbContext.Add(hero);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new ServiceResult<Hero>(hero);
    }

    public async Task<Hero?> GetByIdAsync(Guid heroId, CancellationToken cancellationToken)
    {
        var hero = await _dbContext
            .Heroes
            .Include(x => x.Session)
            .Include(x => x.User)
            .FirstOrDefaultAsync(h => h.HeroId == heroId, cancellationToken);
        
        return hero;
    }

    public async Task<Hero?> GetHeroByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var hero = await _dbContext
            .Heroes
            .Include(x => x.Session)
            .FirstOrDefaultAsync(h => h.UserId == userId, cancellationToken);
        
        return hero;
    }
}