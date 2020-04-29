﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Driver;
using Core.Models;
using Core.Interfaces;
using Core.ViewModels;

namespace InfrastructureMongoDB.Repositories
{
    public class AtividadeMongoDB : IRepositoryAtividade
    {
        private readonly IMongoCollection<Atividade> atividade;
        private readonly IMongoCollection<Counters> counters;
        private readonly IMongoCollection<Categoria> categoria;

        public AtividadeMongoDB(string strconexao)
        {
            var client = new MongoClient(strconexao);
            var database = client.GetDatabase("AtividadesDB");

            atividade = database.GetCollection<Atividade>("Atividade");
            counters = database.GetCollection<Counters>("Counters");
            categoria = database.GetCollection<Categoria>("Categoria");
        }

        private int GetNextSequence(string name)
        {
            var update = Builders<Counters>.Update.Inc("seq", 1);
            var options = new FindOneAndUpdateOptions<Counters, Counters>()
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            return counters.FindOneAndUpdate<Counters>(x => x.Id == name, update, options).seq;            
        }

        public IEnumerable<Atividade> Select()
        {
            return atividade.Find(x => x.DataEncerramento == null).ToList();
        }

        public IEnumerable<Atividade> SelectEncerrados()
        {
            return atividade.Find<Atividade>(x => x.DataEncerramento != null).ToList();
        }

        public Atividade SelectById(int id)
        {
            return atividade.Find<Atividade>(x => x.Id == id).FirstOrDefault();
        }

        public ResultViewModel Insert(Atividade ativ)
        {
            try
            {                
                ativ.Categoria = categoria.Find<Categoria>(x => x.Id == ativ.CategoriaId).FirstOrDefault();
                ativ.Id = GetNextSequence("ativid");
                atividade.InsertOne(ativ);
                
                return new ResultViewModel()
                {
                    Success = true,
                    Message = "Atividade adicionada com sucesso",
                    Data = ativ
                };
            }
            catch (Exception ex)
            {
                return new ResultViewModel()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = ex
                };
            }
        }

        public ResultViewModel Update(Atividade ativ)
        {
            try
            {
                ativ.Categoria = categoria.Find<Categoria>(x => x.Id == ativ.CategoriaId).FirstOrDefault();
                atividade.ReplaceOne(x => x.Id == ativ.Id, ativ);
                return new ResultViewModel()
                {
                    Success = true,
                    Message = "Atividade alterada com sucesso",
                    Data = ativ
                };
            }
            catch (Exception ex)
            {
                return new ResultViewModel()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = ex
                };
            }
        }

        public ResultViewModel UpdateEncerra(Atividade ativ)
        {
            try
            {
                var update = Builders<Atividade>.Update.Set("DataEncerramento", ativ.DataEncerramento);
                atividade.UpdateOne(x => x.Id == ativ.Id, update);
                
                return new ResultViewModel()
                {
                    Success = true,
                    Message = "Atividade alterada com sucesso",
                    Data = ativ
                };
            }
            catch (Exception ex)
            {
                return new ResultViewModel()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = ex
                };
            }
        }

        public ResultViewModel Reabrir(Atividade ativ)
        {
            try
            {
                var update = Builders<Atividade>.Update.Set("DataEncerramento", "");
                atividade.UpdateOne(x => x.Id == ativ.Id, update);
                                
                return new ResultViewModel()
                {
                    Success = true,
                    Message = "Atividade alterada com sucesso",
                    Data = ativ
                };
            }
            catch (Exception ex)
            {
                return new ResultViewModel()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = ex
                };
            }
        }

        public ResultViewModel Delete(Atividade ativ)
        {
            try
            {
                atividade.DeleteOne(x => x.Id == ativ.Id);
                return new ResultViewModel()
                {
                    Success = true,
                    Message = "Atividade eliminada com sucesso",
                    Data = ativ
                };
            }
            catch (Exception ex)
            {
                return new ResultViewModel()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = ex
                };
            }
        }

        public ResultViewModel AlteraPrioridade(JsonPrioridade item)
        {
            try
            {                
                var update = Builders<Atividade>.Update.Set("Prioridade", item.Prioridade);                
                atividade.UpdateOne(x => x.Id == item.Id, update);

                return new ResultViewModel()
                {
                    Success = true,
                    Message = "Atividade alterada com sucesso",
                    Data = item
                };
            }
            catch (Exception ex)
            {
                return new ResultViewModel()
                {
                    Success = false,
                    Message = ex.Message,
                    Data = ex
                };
            }
        }
    }
}