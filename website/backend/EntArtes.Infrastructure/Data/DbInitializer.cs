using EntArtes.Core.Entities;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace EntArtes.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        // Ensure database is created
        await context.Database.MigrateAsync();

        // Seed Modalidades (Dance styles)
        if (!context.Modalidades.Any())
        {
            var modalidades = new Modalidade[]
            {
                new() { Nome = "Ballet Clássico", Descricao = "Técnica clássica com barra e centro" },
                new() { Nome = "Dança Contemporânea", Descricao = "Movimento livre e expressivo" },
                new() { Nome = "Hip Hop", Descricao = "Dança urbana e ritmos modernos" },
                new() { Nome = "Jazz", Descricao = "Energia, giros e saltos" },
                new() { Nome = "Danças Latinas", Descricao = "Salsa, Bachata, Merengue" },
                new() { Nome = "Street Dance", Descricao = "Breaking, Popping, Locking" }
            };
            context.Modalidades.AddRange(modalidades);
            await context.SaveChangesAsync();
        }

        // Seed Estudios (Studios) - 8 studios
        if (!context.Estudios.Any())
        {
            var estudios = new Estudio[]
            {
                new() { Nome = "Estúdio 1", Capacidade = 15, Descricao = "Piso de madeira, espelhos" },
                new() { Nome = "Estúdio 2", Capacidade = 10, Descricao = "Espaço pequeno para aulas individuais" },
                new() { Nome = "Estúdio 3", Capacidade = 20, Descricao = "Sala ampla para ensemble" },
                new() { Nome = "Estúdio 4", Capacidade = 12, Descricao = "Piso de madeira, barra" },
                new() { Nome = "Estúdio 5", Capacidade = 8, Descricao = "Estúdio intimista" },
                new() { Nome = "Estúdio 6", Capacidade = 25, Descricao = "Palco de ensaio" },
                new() { Nome = "Estúdio 7", Capacidade = 15, Descricao = "Espelhos e equipamento de som" },
                new() { Nome = "Estúdio 8", Capacidade = 10, Descricao = "Piso de madeira" }
            };
            context.Estudios.AddRange(estudios);
            await context.SaveChangesAsync();
        }

        // Seed EstudioModalidade (compatibility)
        if (!context.EstudioModalidades.Any())
        {
            // All studios support Ballet and Contemporary
            var estudios = await context.Estudios.ToListAsync();
            var ballet = await context.Modalidades.FirstAsync(m => m.Nome == "Ballet Clássico");
            var contemporanea = await context.Modalidades.FirstAsync(m => m.Nome == "Dança Contemporânea");
            var hipHop = await context.Modalidades.FirstAsync(m => m.Nome == "Hip Hop");
            var jazz = await context.Modalidades.FirstAsync(m => m.Nome == "Jazz");
            var latinas = await context.Modalidades.FirstAsync(m => m.Nome == "Danças Latinas");
            var street = await context.Modalidades.FirstAsync(m => m.Nome == "Street Dance");

            var compatibilities = new List<EstudioModalidade>();

            foreach (var e in estudios)
            {
                // All studios support Ballet and Contemporary
                compatibilities.Add(new EstudioModalidade { EstudioId = e.Id, ModalidadeId = ballet.Id });
                compatibilities.Add(new EstudioModalidade { EstudioId = e.Id, ModalidadeId = contemporanea.Id });

                // Specific assignments (example)
                if (e.Id <= 3) // Studios 1-3 support Hip Hop
                    compatibilities.Add(new EstudioModalidade { EstudioId = e.Id, ModalidadeId = hipHop.Id });
                if (e.Id >= 4 && e.Id <= 6) // Studios 4-6 support Jazz
                    compatibilities.Add(new EstudioModalidade { EstudioId = e.Id, ModalidadeId = jazz.Id });
                if (e.Id >= 5 && e.Id <= 7) // Studios 5-7 support Latinas
                    compatibilities.Add(new EstudioModalidade { EstudioId = e.Id, ModalidadeId = latinas.Id });
                if (e.Id >= 6) // Studios 6-8 support Street
                    compatibilities.Add(new EstudioModalidade { EstudioId = e.Id, ModalidadeId = street.Id });
            }

            context.EstudioModalidades.AddRange(compatibilities);
            await context.SaveChangesAsync();
        }

        // Seed Professors (Utilizador with tipo = Professor)
        if (!context.Utilizadores.Any(u => u.Tipo == TipoUtilizador.Professor))
        {
            var professors = new Utilizador[]
            {
                new() { Nome = "Ana Silva", Email = "ana.silva@entartes.pt", SenhaHash = BCrypt.Net.BCrypt.HashPassword("prof123"), Tipo = TipoUtilizador.Professor },
                new() { Nome = "Carlos Mendes", Email = "carlos.mendes@entartes.pt", SenhaHash = BCrypt.Net.BCrypt.HashPassword("prof123"), Tipo = TipoUtilizador.Professor },
                new() { Nome = "Mariana Costa", Email = "mariana.costa@entartes.pt", SenhaHash = BCrypt.Net.BCrypt.HashPassword("prof123"), Tipo = TipoUtilizador.Professor },
                new() { Nome = "Rui Pereira", Email = "rui.pereira@entartes.pt", SenhaHash = BCrypt.Net.BCrypt.HashPassword("prof123"), Tipo = TipoUtilizador.Professor },
                new() { Nome = "Sofia Rodrigues", Email = "sofia.rodrigues@entartes.pt", SenhaHash = BCrypt.Net.BCrypt.HashPassword("prof123"), Tipo = TipoUtilizador.Professor }
            };
            context.Utilizadores.AddRange(professors);
            await context.SaveChangesAsync();
        }

        // Seed ProfessorModalidade (which professors teach which modalities)
        if (!context.ProfessorModalidades.Any())
        {
            var professors = await context.Utilizadores.Where(u => u.Tipo == TipoUtilizador.Professor).ToListAsync();
            var ballet = await context.Modalidades.FirstAsync(m => m.Nome == "Ballet Clássico");
            var contemporanea = await context.Modalidades.FirstAsync(m => m.Nome == "Dança Contemporânea");
            var hipHop = await context.Modalidades.FirstAsync(m => m.Nome == "Hip Hop");
            var jazz = await context.Modalidades.FirstAsync(m => m.Nome == "Jazz");

            var assignments = new List<ProfessorModalidade>
            {
                new() { ProfessorId = professors[0].Id, ModalidadeId = ballet.Id },
                new() { ProfessorId = professors[0].Id, ModalidadeId = contemporanea.Id },
                new() { ProfessorId = professors[1].Id, ModalidadeId = hipHop.Id },
                new() { ProfessorId = professors[1].Id, ModalidadeId = jazz.Id },
                new() { ProfessorId = professors[2].Id, ModalidadeId = ballet.Id },
                new() { ProfessorId = professors[2].Id, ModalidadeId = jazz.Id },
                new() { ProfessorId = professors[3].Id, ModalidadeId = contemporanea.Id },
                new() { ProfessorId = professors[3].Id, ModalidadeId = hipHop.Id },
                new() { ProfessorId = professors[4].Id, ModalidadeId = ballet.Id },
                new() { ProfessorId = professors[4].Id, ModalidadeId = contemporanea.Id }
            };
            context.ProfessorModalidades.AddRange(assignments);
            await context.SaveChangesAsync();
        }

        // Seed DisponibilidadeProfessor (availability)
        if (!context.DisponibilidadesProfessores.Any())
        {
            var professors = await context.Utilizadores.Where(u => u.Tipo == TipoUtilizador.Professor).ToListAsync();
            var availabilities = new List<DisponibilidadeProfessor>();

            // Example: Professor 1: Monday 14h-18h, Wednesday 10h-13h
            availabilities.Add(new DisponibilidadeProfessor { ProfessorId = professors[0].Id, DiaSemana = 1, HoraInicio = new TimeSpan(14, 0, 0), HoraFim = new TimeSpan(18, 0, 0), Recorrente = true });
            availabilities.Add(new DisponibilidadeProfessor { ProfessorId = professors[0].Id, DiaSemana = 3, HoraInicio = new TimeSpan(10, 0, 0), HoraFim = new TimeSpan(13, 0, 0), Recorrente = true });

            // Professor 2: Tuesday 15h-19h, Thursday 16h-20h
            availabilities.Add(new DisponibilidadeProfessor { ProfessorId = professors[1].Id, DiaSemana = 2, HoraInicio = new TimeSpan(15, 0, 0), HoraFim = new TimeSpan(19, 0, 0), Recorrente = true });
            availabilities.Add(new DisponibilidadeProfessor { ProfessorId = professors[1].Id, DiaSemana = 4, HoraInicio = new TimeSpan(16, 0, 0), HoraFim = new TimeSpan(20, 0, 0), Recorrente = true });

            // Professor 3: Monday 9h-12h, Wednesday 14h-18h
            availabilities.Add(new DisponibilidadeProfessor { ProfessorId = professors[2].Id, DiaSemana = 1, HoraInicio = new TimeSpan(9, 0, 0), HoraFim = new TimeSpan(12, 0, 0), Recorrente = true });
            availabilities.Add(new DisponibilidadeProfessor { ProfessorId = professors[2].Id, DiaSemana = 3, HoraInicio = new TimeSpan(14, 0, 0), HoraFim = new TimeSpan(18, 0, 0), Recorrente = true });

            // Add more as needed
            context.DisponibilidadesProfessores.AddRange(availabilities);
            await context.SaveChangesAsync();
        }

        // Seed sample Encarregado (parent) and Alunos
        if (!context.Utilizadores.Any(u => u.Tipo == TipoUtilizador.Encarregado))
        {
            var enc = new Utilizador
            {
                Nome = "João Pai",
                Email = "pai@example.com",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Tipo = TipoUtilizador.Encarregado
            };
            context.Utilizadores.Add(enc);
            await context.SaveChangesAsync();

            var alunos = new Aluno[]
            {
                new() { Nome = "Maria Aluna", EncarregadoId = enc.Id },
                new() { Nome = "Pedro Aluno", EncarregadoId = enc.Id }
            };
            context.Alunos.AddRange(alunos);
            await context.SaveChangesAsync();
        }
    }
}
