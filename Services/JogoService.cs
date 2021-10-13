using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiCatalogoJogos.Entities;
using ApiCatalogoJogos.Exceptions;
using ApiCatalogoJogos.InputModel;
using ApiCatalogoJogos.Repositories;
using ApiCatalogoJogos.ViewModel;

namespace ApiCatalogoJogos.Services
{
    public class JogoService : IJogoService
    {
        private readonly IJogoRepository _jogoRepository;

        public JogoService(IJogoRepository jogoRepository)
        {
            _jogoRepository = jogoRepository;
        }

        public async Task<List<JogoViewModel>> Obter(int pagina, int quantidade) 
        {
            var jogos = await _jogoRepository.Obter(pagina, quantidade); //Busca os jogos.

            return jogos.Select(jogo => new JogoViewModel //Fazendo uma implementação com link usando o select.
            {
                Id = jogo.Id,
                Nome = jogo.Nome,
                Produtora = jogo.Produtora,
                Preco = jogo.Preco
            }).ToList(); //Pra cada jogo, cria um jogoViewModel e cria uma lista.
        }

        public async Task<JogoViewModel> Obter(Guid id)
        {
            var jogo = await _jogoRepository.Obter(id);

            if (jogo == null)
                return null;

            return new JogoViewModel
            {
                Id = jogo.Id,
                Nome = jogo.Nome,
                Produtora = jogo.Produtora,
                Preco = jogo.Preco
            };
        }

        public async Task<JogoViewModel> Inserir(JogoInputModel jogo)
        {
            var entidadeJogo = await _jogoRepository.Obter(jogo.Nome, jogo.Produtora); //Vai buscar um jogo com tal nome e produtora.

            if (entidadeJogo.Count > 0)
                throw new JogoJaCadastradoException(); //Se exixtir algum jogo cadastrado com esse nome e produtora, a service retorna erro.

            var jogoInsert = new Jogo //Se não, ele vai criar uma nova entidade,
            {
                Id = Guid.NewGuid(), //gerando um novo código pra esse jogo,
                Nome = jogo.Nome,
                Produtora = jogo.Produtora,
                Preco = jogo.Preco
            };

            await _jogoRepository.Inserir(jogoInsert); //e aí vai inserir, chamar o repositório inserindo ele,

            return new JogoViewModel //e depois vai retornar uma ViewModel, com o Id inserido no jogoInserir,
            {
                Id = jogoInsert.Id, //pra que quem chamou essa requisição, saiba qual é o id desse novo recurso.
                Nome = jogo.Nome,
                Produtora = jogo.Produtora,
                Preco = jogo.Preco
            };
        }

        public async Task Atualizar(Guid id, JogoInputModel jogo) //Vai receber o id e o JogoInputModel.
        {
            var entidadeJogo = await _jogoRepository.Obter(id); //Vai tentar obter esse jogo.

            if (entidadeJogo == null)
                throw new JogoNaoCadastradoException(); //Caso o jogo n exista ele vai dar uma exceção.

            entidadeJogo.Nome = jogo.Nome; //Senão, ele vai atualizar através das informações passadas na entidade.
            entidadeJogo.Produtora = jogo.Produtora; //A entidade vai trocar os valores dela pelos valore chegando da JogoInputModel,
            entidadeJogo.Preco = jogo.Preco;

            await _jogoRepository.Atualizar(entidadeJogo); //E vai mandar atualizar
        }

        public async Task Atualizar(Guid id, double preco) //Garante que vai trocar só o preço
        {
            var entidadeJogo = await _jogoRepository.Obter(id);

            if (entidadeJogo == null)
                throw new JogoNaoCadastradoException();

            entidadeJogo.Preco = preco;

            await _jogoRepository.Atualizar(entidadeJogo);
        }

        public async Task Remover(Guid id)
        {
            var jogo = await _jogoRepository.Obter(id);

            if (jogo == null)
                throw new JogoNaoCadastradoException();

            await _jogoRepository.Remover(id);
        }

        public void Dispose() //controlar a destruição do objeto.
        {
            _jogoRepository?.Dispose(); //quando ele estiver sendo destruído eu quero que feche as conexões de repositório.
        }//Por exemplo, nosso repositório ficou com a conexão aberta pro banco de dados.imagine que seu banco tem o máximo de 200 conexões.
         //isso pode gerar problema.
    }
}
