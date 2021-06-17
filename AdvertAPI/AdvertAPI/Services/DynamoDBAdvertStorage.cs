using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvertAPI.Models;
using AutoMapper;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace AdvertAPI.Services
{
    public class DynamoDBAdvertStorage : IAdvertStorageService
    {
        private readonly IMapper _mapper;
        public DynamoDBAdvertStorage(IMapper mapper)
        {
            _mapper = mapper;
        }
        public async Task<string> Add(AdvertModel model)
        {
            var dbModel = _mapper.Map<AdvertModelDb>(model);
            dbModel.Id = Guid.NewGuid().ToString();
            dbModel.CreationDate = DateTime.UtcNow;
            dbModel.Status = AdvertStatus.Pending;

            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    await context.SaveAsync(dbModel);
                }
            }
            return dbModel.Id;
        }

        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                using (var client = new AmazonDynamoDBClient())
                {
                    var tableData = await client.DescribeTableAsync("Adverts");
                    return string.Compare(tableData.Table.TableStatus, "active", true) == 0;
                }
            }
            catch(Exception ex)
            {
                return false;
            }

        }

        public async Task Confirm(ConfirmAdvertModel model)
        {

            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    var record = await context.LoadAsync<AdvertModelDb>(model.Id);
                    if(record == null)
                    {
                        throw new KeyNotFoundException("Record Not Found");
                    }
                    if(model.Status == AdvertStatus.Active)
                    {
                        record.Status = AdvertStatus.Active;
                        await context.SaveAsync(record);
                    } else
                    {
                        await context.DeleteAsync(record);
                    }
                }
            }
        }
        public async Task<AdvertModel> GetByIdAsync(string id)
        {
            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    var dbModel = await context.LoadAsync<AdvertModelDb>(id);
                    if (dbModel != null) return _mapper.Map<AdvertModel>(dbModel);
                }
            }

            throw new KeyNotFoundException();
        }
    }
}
