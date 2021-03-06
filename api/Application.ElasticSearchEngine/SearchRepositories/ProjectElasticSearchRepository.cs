﻿
using LMPlatform.ElasticDataModels;
using LMPlatform.Models;
using Nest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Application.ElasticSearchEngine.SearchRepositories
{
    public class ProjectElasticSearchRepository : BaseElasticSearchRepository
    {
        private static string PROJECTS_INDEX_NAME => ConfigurationManager.AppSettings["ProjectsIndexName"];

        public ProjectElasticSearchRepository(ElasticClient client)
            : base(client, PROJECTS_INDEX_NAME)
        { }
        public ProjectElasticSearchRepository(string elastickAddress, string userName, string password, int prefixLength = 3, int fuzziness = 6)
            : base(elastickAddress, PROJECTS_INDEX_NAME, prefixLength, fuzziness, userName, password)
        { }
       
        public IEnumerable<ElasticProject> Search(string requestStr)
        {
            string searchStr = "";
            if (requestStr != null)
            {
                searchStr = requestStr.ToLower();
            }
            var searchResponse = Client.Search<ElasticProject>(s => s
            .Index(PROJECTS_INDEX_NAME)
            .Size(DEFAULT_NUM_OF_RESULTS)
            .Query(q => q
                      .Match(m => m
                          .Fuzziness(Fuzziness.EditDistance(SearchFuziness))
                          .PrefixLength(PrefixLength)
                          .Field(p => p.Title)
                          .Field(p => p.Details)
                          .FuzzyTranspositions(true)
                          .Query(searchStr)
                      )
                      || q
                      .Prefix(p => p.Title , searchStr)
                      || q
                      .Prefix(p => p.Details , searchStr)
             )   
            ); ;
            return searchResponse.Documents.ToList<ElasticProject>();
        }
        public IEnumerable<ElasticProject> SearchAll()
        {
            var searchResponse = Client.Search<ElasticProject>(s => s
            .Index(PROJECTS_INDEX_NAME)
            .Size(200)
            .From(0)
            .Query(q => q
                .Nested(n=>n
                    .Query(u=>u
                        .MatchAll()
                        )
                    )
                )
            );
            Console.WriteLine("found {0} documents", searchResponse.Documents.Count);
            return searchResponse.Documents.ToList<ElasticProject>();
        }
        public void AddToIndex(ElasticProject project)
        {
                Client.Index<ElasticProject>(project, st => st.Index(PROJECTS_INDEX_NAME));
        }
        public void AddToIndex(IEnumerable<ElasticProject> projects)
        {
            Client.IndexMany(projects, PROJECTS_INDEX_NAME);
        }
        public void AddToIndexAsync(ElasticProject project)
        {
            Client.IndexAsync<ElasticProject>(project, st => st.Index(PROJECTS_INDEX_NAME));
        }
        public void AddToIndexAsync(IEnumerable<ElasticProject> projects)
        {
            Client.IndexManyAsync(projects, PROJECTS_INDEX_NAME);
        }
        public void UpdateDocument(ElasticProject project)
        {
            DeleteFromIndex(project.Id);
            AddToIndex(project);
        }

        internal static CreateIndexDescriptor GetProjectMap(string indexName)
        {
            CreateIndexDescriptor map = new CreateIndexDescriptor(indexName);
            map.Map<ElasticProject>(m => m
                    .Dynamic(false)
                    .Properties(prop => prop
                        .Number(s => s
                            .Name(n => n.Id)
                            .Type(NumberType.Integer)
                        )
                        .Date(s => s
                            .Name(n => n.DateOfChange)
                        )
                        .Text(s => s
                            .Name(n => n.Title)
                        )
                   )
                )
           ;
            return map;
        }
    }
}
    

