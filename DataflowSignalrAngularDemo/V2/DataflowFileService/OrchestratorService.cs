using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataflowFileService
{
    public class OrchestratorService
    {
        private BufferBlock<SalesOrderDetailEntity> _filesBufferBlock;
        private readonly BufferBlock<OrderEntity> _bufferBlock = new BufferBlock<OrderEntity>();
        private TransformManyBlock<OrderEntity, OrderDetailEntity> _receptorBlockOne;
        private TransformManyBlock<OrderEntity, OrderDetailEntity> _receptorBlockTwo;
        private TransformManyBlock<OrderEntity, OrderDetailEntity> _receptorBlockThee;
        private ActionBlock<SalesOrderDetailEntity> _loggerBlock;

        /// <summary>
        /// </summary>
        private TransformBlock<OrderDetailEntity, SalesOrderDetailEntity> _transformBlockCalculateOrderDetail;

        /// <summary>
        /// </summary>
        private readonly ExecutionDataflowBlockOptions _blockConfiguration = new ExecutionDataflowBlockOptions()
        {
            NameFormat = "Type:{0},Id:{1}",
            MaxDegreeOfParallelism = 4,
        };

        public async Task Execute()
        {
            BuildFileReceptionWorkflow();
            await ExtractOrder();
        }

        private Task<SalesOrderDetailEntity> CalculateOrderDetail(OrderDetailEntity orderDetailEntity)
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
            PostData(orderEntity, "FileOrderEntity");
            return Task.Run(() => orderEntity);
        }

        /// <summary>
        /// </summary>
        private async Task ExtractOrder()
        {
            await BroadCastData();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private async Task BroadCastData()
        {
            //for (int i = 0; i < 100; i++)
            //{
            //    _bufferBlock.Post(i.ToString());
            //}

            //await Task.Run(() =>
            //                    {
            //                        for (int i = 0; i < 10000; i++)
            //                        {
            //                            _bufferBlock.Post(new OrderEntity
            //                            {
            //                                OrderID = i,
            //                                Products = new List<Product>
            //                                {
            //                                    new Product
            //                                    {
            //                                        ProductID = new Random().Next(i),
            //                                        Price = new Random().Next(10,1000),
            //                                        Qty = new Random().Next(1,50)
            //                                    }
            //                                },
            //                                AccountNumber = Guid.NewGuid(),
            //                                SalesPersonId = Guid.NewGuid()
            //                            });
            //                        }
            //                    }
            //);
            await Task.Run(() =>
            Parallel.For(0, 100000,
                    i =>
                    {
                        var price = i < 1000 ? i : 999;
                        _bufferBlock.Post(new OrderEntity
                        {
                            OrderID = Guid.NewGuid(),
                            Products = new List<Product>
                                            {
                                                new Product
                                                {
                                                    ProductID = Guid.NewGuid(),
                                                    Price = new Random().Next(price,1000),
                                                    Qty = new Random().Next(1,50)
                                                }
                                            },
                            AccountNumber = Guid.NewGuid(),
                            SalesPersonId = Guid.NewGuid()
                        });
                    })
             );
        }

        /// <summary>
        ///
        /// </summary>
        public void BuildFileReceptionWorkflow()
        {
            var nonGreedy = new ExecutionDataflowBlockOptions() { BoundedCapacity = 1 };
            var flowComplete = new DataflowLinkOptions() { PropagateCompletion = true };

            _transformBlockCalculateOrderDetail = new TransformBlock<OrderDetailEntity, SalesOrderDetailEntity>(x => CalculateOrderDetail(x), _blockConfiguration);

            _transformBlockCalculateOrderDetail
               .Completion
               .ContinueWith(
                   dbt =>
                   {
                       if (dbt.Exception != null)
                           foreach (Exception error in dbt.Exception.Flatten().InnerExceptions)
                           {
                               Console.WriteLine("_transformBlockAutorizeVendor block failed Reason:{0}", error.Message);
                           }
                   },
                   TaskContinuationOptions.OnlyOnFaulted);

            _loggerBlock = new ActionBlock<SalesOrderDetailEntity>(x => Console.WriteLine(string.Format(" Printing {0}", x.SalesPersonName)));

            _filesBufferBlock = new BufferBlock<SalesOrderDetailEntity>();
            _receptorBlockTwo = new TransformManyBlock<OrderEntity, OrderDetailEntity>(i => FindOrders("ReceiverA", i), nonGreedy);
            _receptorBlockThee = new TransformManyBlock<OrderEntity, OrderDetailEntity>(i => FindOrders("ReceiverB", i), nonGreedy);
            _receptorBlockOne = new TransformManyBlock<OrderEntity, OrderDetailEntity>(i => FindOrders("ReceiverC", i), nonGreedy);

            _bufferBlock.LinkTo(_receptorBlockOne, flowComplete);
            _bufferBlock.LinkTo(_receptorBlockTwo, flowComplete);
            _bufferBlock.LinkTo(_receptorBlockThee, flowComplete);

            _receptorBlockOne.LinkTo(_transformBlockCalculateOrderDetail);
            _receptorBlockTwo.LinkTo(_transformBlockCalculateOrderDetail);
            _receptorBlockThee.LinkTo(_transformBlockCalculateOrderDetail);
            _transformBlockCalculateOrderDetail.LinkTo(_filesBufferBlock);
            _filesBufferBlock.LinkTo(_loggerBlock);
            _transformBlockCalculateOrderDetail.Completion.ContinueWith(t => _filesBufferBlock.Complete());
        }

        /// <summary>
        /// </summary>
        /// <param name="receiverName"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private IEnumerable<OrderDetailEntity> FindOrders(string receiverName, OrderEntity orderEntity)
        {
            Console.WriteLine("Processor {0}, starting : {1}", receiverName, orderEntity);
            var processor = new Processor
            {
                Name = receiverName,
                OrderId = orderEntity.OrderID.ToString(),
                Date = DateTime.Now
            };

            PostData(processor, "Processor");

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

        ///   <summary>
        ///
        ///   </summary>
        ///  <param name="processor"></param>
        /// <param name="routePrefix"></param>
        private void PostData<T>(T processor, string routePrefix)
        {
            //var item = processor as Processor;
            //if (item != null)
            //{
            //    Console.WriteLine($"{item.Name} : {item.OrderId}");
            //}

            //var item2 = processor as SalesOrderDetailEntity;
            //if (item2 != null)
            //{
            //    Console.WriteLine($"{item2.SalesOrderName} : {item2.ProductID}");
            //}
            var json = JsonConvert.SerializeObject(processor);
            var apiPath = ConfigurationManager.AppSettings["ServerUrl"];
            // Post the data to the server
            var serverUrl = new Uri(Path.Combine(apiPath, routePrefix));

            var client = new WebClient();
            client.Headers.Add("Content-Type", "application/json;charset=utf-8");
            Task.Run(() => client.UploadString(serverUrl, json));
        }
    }
}