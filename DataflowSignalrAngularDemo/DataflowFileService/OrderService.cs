using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataflowFileService
{
    public class OrderService
    {
        private readonly ExecutionDataflowBlockOptions _blockConfiguration =
            new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
            };

        private readonly BufferBlock<OrderEntity> _bufferBlock = new BufferBlock<OrderEntity>();
        private TransformManyBlock<OrderEntity, OrderDetailEntity> _receptorBlockOne;
        private TransformManyBlock<OrderEntity, OrderDetailEntity> _receptorBlockTwo;
        private TransformManyBlock<OrderEntity, OrderDetailEntity> _receptorBlockThee;
        private ActionBlock<SalesOrderDetailEntity> _loggerBlock;

        private TransformBlock<OrderDetailEntity, SalesOrderDetailEntity>
                                                          _transformBlockCalculateSalesOrderDetail;

        private BufferBlock<SalesOrderDetailEntity> _salesOrderDetailEntityBufferBlock;

        public async Task Execute()
        {
            BuildFileReceptionWorkflow();
            await ExtractOrder();
        }

        /// <summary>
        /// </summary>
        private async Task ExtractOrder()
        {
            while (true)
            {
                await BroadCastData();
                await Task.Delay(1000);
            }
        }

        private async Task BroadCastData()
        {
            await Task.Run(() =>
            {
                int count = 10;
                Parallel.For(0, count++,
                        i =>
                        {
                            var price = i < 1000 ? i : 999;
                            var qty = i < 10000 ? i : 9999;
                            _bufferBlock.Post(new OrderEntity
                            {
                                OrderID = Guid.NewGuid(),
                                Products = new List<Product>
                                                {
                                                new Product
                                                {
                                                    ProductID = Guid.NewGuid(),
                                                    Price = new Random().Next(price,1000),
                                                    Qty =   new Random().Next(qty,10000)
                                                }
                                                },
                                AccountNumber = Guid.NewGuid(),
                                SalesPersonId = Guid.NewGuid()
                            });
                        });
            });
        }

        public void BuildFileReceptionWorkflow()
        {
            var nonGreedy = new ExecutionDataflowBlockOptions() { BoundedCapacity = 1 };

            var flowComplete = new DataflowLinkOptions() { PropagateCompletion = true };

            _transformBlockCalculateSalesOrderDetail = new TransformBlock<OrderDetailEntity, SalesOrderDetailEntity>
                (x => CalculateOrderDetail(x), _blockConfiguration);

            _transformBlockCalculateSalesOrderDetail
               .Completion
               .ContinueWith(
                   dbt =>
                   {
                       if (dbt.Exception != null)
                           foreach (Exception error in dbt.Exception.Flatten().InnerExceptions)
                           {
                               Console.WriteLine("_transformBlockCalculateOrderDetail block failed Reason:{0}", error.Message);
                           }
                   },
                   TaskContinuationOptions.OnlyOnFaulted);

            _loggerBlock = new ActionBlock<SalesOrderDetailEntity>(x => Console.WriteLine(string.Format(" Printing {0}",
                x.SalesPersonName)));

            _salesOrderDetailEntityBufferBlock = new BufferBlock<SalesOrderDetailEntity>();
            _receptorBlockTwo = new TransformManyBlock<OrderEntity, OrderDetailEntity>
                                                                            (i => FindOrders("Receiver A", i), nonGreedy);
            _receptorBlockThee = new TransformManyBlock<OrderEntity, OrderDetailEntity>
                                                                            (i => FindOrders("Receiver B", i), nonGreedy);
            _receptorBlockOne = new TransformManyBlock<OrderEntity, OrderDetailEntity>
                                                                             (i => FindOrders("Receiver C", i), nonGreedy);

            _bufferBlock.LinkTo(_receptorBlockOne, flowComplete);
            _bufferBlock.LinkTo(_receptorBlockTwo, flowComplete);
            _bufferBlock.LinkTo(_receptorBlockThee, flowComplete);

            _receptorBlockOne.LinkTo(_transformBlockCalculateSalesOrderDetail, flowComplete);
            _receptorBlockTwo.LinkTo(_transformBlockCalculateSalesOrderDetail, flowComplete);
            _receptorBlockThee.LinkTo(_transformBlockCalculateSalesOrderDetail, flowComplete);
            _transformBlockCalculateSalesOrderDetail.LinkTo(_salesOrderDetailEntityBufferBlock, flowComplete);
            _salesOrderDetailEntityBufferBlock.LinkTo(_loggerBlock, flowComplete);

            Task.WhenAll(_salesOrderDetailEntityBufferBlock.Completion,
                         _transformBlockCalculateSalesOrderDetail.Completion)
                         .ContinueWith(c => _bufferBlock.Complete());
        }

        private async Task<IEnumerable<OrderDetailEntity>> FindOrders(string receiverName, OrderEntity orderEntity)
        {
            Console.WriteLine("Processor {0}, starting : {1}", receiverName, orderEntity);
            var processor = new Processor
            {
                Name = receiverName,
                OrderId = orderEntity.OrderID.ToString(),
                Date = DateTime.Now
            };

            await PostData(processor, "Processor");

            return new List<OrderDetailEntity>
            {
                new OrderDetailEntity
                {
                     OrderID = orderEntity.OrderID,
                     Origin = receiverName,
                     Account = new Account
                     {
                           AccountNumber = orderEntity.AccountNumber
                     },
                     OrderDate = DateTime.Now,
                     PriceDiscount = 10,
                     Products = orderEntity.Products,
                     SalesPerson = new SalesPerson
                     {
                         SalesPersonId = orderEntity.SalesPersonId
                     },
                     Processor = processor
                }
            };
        }

        private async Task<SalesOrderDetailEntity> CalculateOrderDetail(OrderDetailEntity orderDetailEntity)
        {
            var orderEntity = new SalesOrderDetailEntity
            {
                Products = orderDetailEntity.Products,
                SalesOrderDetailID = Guid.NewGuid(),
                OrderQty = orderDetailEntity.Products.Sum(p => p.Qty),
                PriceDiscount = orderDetailEntity.PriceDiscount,
                LineTotal = orderDetailEntity.Products.Sum(p => p.Price * p.Qty),
                CarrierTrackingNumber = $"CarrierTrackingNumber_{Guid.NewGuid()}",
                Date = orderDetailEntity.OrderDate,
                SalesPersonName = orderDetailEntity.SalesPerson.Name,
            };
            Console.WriteLine("CalculateOrderDetail :  {0}", orderDetailEntity.OrderID);
            await PostData(orderEntity, "SalesOrderDetailEntity");
            return await Task.Run(() => orderEntity);
        }

        private async Task PostData<T>(T processor, string routePrefix)
        {
            var apiPath = ConfigurationManager.AppSettings["ServerUrl"];
            var serverUrl = Path.Combine(apiPath, routePrefix);
            await CreateAsync(processor, serverUrl);
        }

        public async Task<Uri> CreateAsync<T>(T item, string url)
        {
            try
            {
                HttpClient _client = new HttpClient();
                HttpResponseMessage response = await _client.PostAsJsonAsync(url, item);
                response.EnsureSuccessStatusCode();
                return response.Headers.Location;
            }
            catch (Exception ex)
            {
                //Log here
                return new Uri(url);
            }
        }
    }
}