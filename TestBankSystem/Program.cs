using BankSystem.AppContext;
using Microsoft.EntityFrameworkCore;

BankContext bankContext = new BankContext(new DbContextOptions<BankContext>());
