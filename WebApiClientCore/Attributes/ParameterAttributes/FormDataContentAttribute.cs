﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace WebApiClientCore.Attributes
{
    /// <summary>
    /// 使用KeyValueSerializer序列化参数值得到的键值作为 multipart/form-data 表单
    /// </summary>
    public class FormDataContentAttribute : HttpContentAttribute, ICollectionFormatable
    {
        /// <summary>
        /// 获取或设置集合格式化方式
        /// </summary>
        public CollectionFormat CollectionFormat { get; set; } = CollectionFormat.Multi;

        /// <summary>
        /// 设置参数到 http 请求内容
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns>
        protected override Task SetHttpContentAsync(ApiParameterContext context)
        {
            var keyValues = this.SerializeToKeyValues(context).CollectAs(this.CollectionFormat);
            context.HttpContext.RequestMessage.AddFormDataText(keyValues);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 序列化参数为 keyValue 集合
        /// </summary>
        /// <param name="context">上下文</param>
        /// <returns></returns> 
        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        [UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "<Pending>")]
        protected virtual IEnumerable<KeyValue> SerializeToKeyValues(ApiParameterContext context)
        {
            return context.SerializeToKeyValues();
        }
    }
}
