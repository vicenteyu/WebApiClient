﻿using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebApiClientCore.Internals;

namespace WebApiClientCore.HttpContents
{
    /// <summary>
    /// 表示缓冲的 Http 内容
    /// </summary>
    public class BufferContent : HttpContent, IBufferWriter<byte>
    {
        /// <summary>
        /// buffer
        /// </summary>
        private readonly RecyclableBufferWriter<byte> bufferWriter = new();

        /// <summary>
        /// 获取已数入的数据
        /// </summary>
        protected ArraySegment<byte> WrittenSegment => this.bufferWriter.WrittenSegment;

        /// <summary>
        /// 缓冲的 Http 内容
        /// </summary> 
        public BufferContent(string mediaType)
            : this(new MediaTypeHeaderValue(mediaType))
        {
        }

        /// <summary>
        /// 缓冲的 Http 内容
        /// </summary> 
        public BufferContent(MediaTypeHeaderValue mediaType)
        {
            this.Headers.ContentType = mediaType;
        }

        /// <summary>
        /// 缓冲的 Http 内容
        /// </summary>
        protected BufferContent()
        {
        }

        /// <summary>
        /// 设置向前推进实际写入的数据长度
        /// </summary>
        /// <param name="count">数据长度</param>
        public void Advance(int count)
        {
            this.bufferWriter.Advance(count);
        }

        /// <summary>
        /// 返回用于写入数据的Memory
        /// </summary>
        /// <param name="sizeHint">预计数据大小</param>
        /// <returns></returns>
        public Memory<byte> GetMemory(int sizeHint)
        {
            this.EnsureNotBuffered();
            return this.bufferWriter.GetMemory(sizeHint);
        }

        /// <summary>
        /// 返回用于写入数据的Span
        /// </summary>
        /// <param name="sizeHint">预计数据大小</param>
        /// <returns></returns>
        public Span<byte> GetSpan(int sizeHint)
        {
            this.EnsureNotBuffered();
            return this.bufferWriter.GetSpan(sizeHint);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="buffer">数据</param>
        public void Write(byte buffer)
        {
            this.EnsureNotBuffered();
            this.bufferWriter.Write(buffer);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="buffer">数据</param>
        public void Write(Span<byte> buffer)
        {
            this.EnsureNotBuffered();
            this.bufferWriter.Write(buffer);
        }

        /// <summary>
        /// 清除数据
        /// </summary>
        public void Clear()
        {
            this.bufferWriter.Clear();
        }

        /// <summary>
        /// 创建只读流
        /// </summary>
        /// <returns></returns>
        protected override Task<Stream> CreateContentReadStreamAsync()
        {
            var segment = this.bufferWriter.WrittenSegment;
            var readStream = new MemoryStream(segment.Array!, segment.Offset, segment.Count, writable: false);
            return Task.FromResult<Stream>(readStream);
        }

        /// <summary>
        /// 序列化到目标流中
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            var memory = this.bufferWriter.WrittenMemory;
            return stream.WriteAsync(memory).AsTask();
        }

        /// <summary>
        /// 计算内容长度
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        protected override bool TryComputeLength(out long length)
        {
            length = this.bufferWriter.WrittenCount;
            return true;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.bufferWriter.Dispose();
            base.Dispose(disposing);
        }
    }
}
