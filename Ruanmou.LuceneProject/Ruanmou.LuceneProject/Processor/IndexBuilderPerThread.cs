﻿using Ruanmou.LuceneProject.DataService;
using Ruanmou.LuceneProject.Interface;
using Ruanmou.LuceneProject.Model;
using Ruanmou.LuceneProject.Service;
using Ruanmou.LuceneProject.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Ruanmou.LuceneProject.Processor
{
    public class IndexBuilderPerThread
    {
        private Logger logger = new Logger(typeof(IndexBuilderPerThread));
        private int CurrentThreadNum = 0;
        private string PathSuffix = "";
        private CancellationTokenSource CTS = null;
        /// <summary>
        /// 为了避免多线程临时变量问题，将参数在构造时就传进来
        /// </summary>
        /// <param name="threadNum"></param>
        /// <param name="pathSuffix"></param>
        /// <param name="cts"></param>
        public IndexBuilderPerThread(int threadNum, string pathSuffix, CancellationTokenSource cts)
        {
            this.CurrentThreadNum = threadNum;
            this.PathSuffix = pathSuffix;
            this.CTS = cts;
        }

        public void Process()
        {
            try
            {
                logger.Debug(string.Format("ThreadNum={0}开始创建", CurrentThreadNum));
                CommodityRepository commodityRepository = new CommodityRepository();
                ILuceneBulid builder = new LuceneBulid();
                bool isFirst = true;
                int pageIndex = 1;
                while (!CTS.IsCancellationRequested)
                {
                    List<Commodity> commodityList = commodityRepository.QueryList(CurrentThreadNum, pageIndex, 1000);
                    if (commodityList == null || commodityList.Count == 0)
                    {
                        break;
                    }
                    //else if (pageIndex == 11)
                    //{
                    //    break;//为了测试  只做10000条数据
                    //}
                    else
                    {
                        builder.BuildIndex(commodityList, PathSuffix, isFirst);
                        logger.Debug(string.Format("ThreadNum={0}完成{1}条的创建", CurrentThreadNum, 1000 * pageIndex++));
                        isFirst = false;
                    }
                }
            }
            catch (Exception ex)
            {
                CTS.Cancel();
                logger.Error(string.Format("ThreadNum={0}出现异常", CurrentThreadNum), ex);
            }
            finally
            {
                logger.Debug(string.Format("ThreadNum={0}完成创建", CurrentThreadNum));
            }
        }
    }
}
