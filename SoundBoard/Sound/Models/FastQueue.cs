using System.Buffers;
using System.Collections.Concurrent;

namespace SoundBoard.Sound.Models;

public class FastQueue
{
    private int _bigArrayIndex = 0;
    private int _bigArrayOffset = 0;
    private readonly ConcurrentQueue<Sample> _queue = [];
    private readonly float[] _bigArray = new float[ushort.MaxValue];
    private readonly ArrayPool<float> _floatArrayPool = ArrayPool<float>.Shared;
    private static void WrapCopy(
        float[] source,
        int sourceIndex,
        float[] target,
        int targetIndex,
        int length)
    {
        sourceIndex = sourceIndex % source.Length;
        targetIndex = targetIndex % target.Length;
        
        var maxSourceLength = source.Length - sourceIndex;
        var maxTargetLength = target.Length - targetIndex;
        
        if (sourceIndex + length > source.Length)
        {
            if (targetIndex + length > target.Length)
            {
                if (maxSourceLength > maxTargetLength)
                {
                    // 5, S: [ 1, 1, 0, [X, 1], 1 ] -> T: [ 0, 0, 0, 0, [A, 0] ]
                    //                              -> T: [ 0, 0, 0, 0, [X, 1] ]
                    Array.Copy(source, sourceIndex, target, targetIndex, maxTargetLength);
                    
                    // 5, S: [ 1, 1, 0, X, 1, [1] ] -> T: [ [0], 0, 0, 0, A, 0 ]
                    //                              -> T: [ [1], 0, 0, 0, X, 1 ]
                    Array.Copy(source, sourceIndex + maxTargetLength, target, 0, maxSourceLength - maxTargetLength);
                    
                    // 5, S: [ [1, 1], 0, X, 1, 1 ] -> T: [ 1, [0, 0], 0, A, 0 ]
                    //                              -> T: [ 0, [1, 1], 0, X, 1 ]
                    Array.Copy(source, 0, target, maxSourceLength - maxTargetLength, length - maxSourceLength);
                }
                else
                {
                    // 5, S: [ 1, 1, 1, 0, [X, 1] ] -> T: [ 0, 0, 0, [A, 0], 0 ]
                    //                              -> T: [ 0, 0, 0, [X, 1], 0 ]
                    Array.Copy(source, sourceIndex, target, targetIndex, maxSourceLength);
                    
                    // 5, S: [ [1], 1, 1, 0, X, 1 ] -> T: [ 0, 0, 0, X, 1, [0] ]
                    //                              -> T: [ 0, 0, 0, X, 1, [1] ]
                    Array.Copy(source, 0, target, targetIndex + maxSourceLength, maxTargetLength - maxSourceLength);
                    
                    // 5, S: [ 1, [1, 1], 0, X, 1 ] -> T: [ [0, 0], 0, X, 1, 1 ]
                    //                              -> T: [ [1, 1], 0, X, 1, 1 ]
                    Array.Copy(source, maxTargetLength - maxSourceLength, target, 0, length - maxTargetLength);
                }
            }
            else
            {
                // 5, S: [ 1, 1, 1, 0, [X, 1] ] -> T: [ [A, 0], 0, 0, 0, 0 ]
                //                              -> T: [ [X, 1], 0, 0, 0, 0 ]
                Array.Copy(source, sourceIndex, target, targetIndex, maxSourceLength);
                    
                // 5, S: [ [1, 1, 1], 0, X, 1 ] -> T: [ X, 1, [0, 0, 0], 0 ]
                //                              -> T: [ X, 1, [1, 1, 1], 0 ]
                Array.Copy(source, sourceIndex, target, targetIndex + maxSourceLength, length - maxSourceLength);
            }
        }
        else
        {
            if (targetIndex + length > target.Length)
            {
                // 5, S: [ [X, 1], 1, 1, 1, 0 ] -> T: [ 0, 0, 0, 0, [A, 0] ]
                //                              -> T: [ 0, 0, 0, 0, [X, 1] ]
                Array.Copy(source, sourceIndex, target, targetIndex, maxTargetLength);
                
                // 5, S: [ X, 1, [1, 1, 1], 0 ] -> T: [ [0, 0, 0], 0, X, 1 ]
                //                              -> T: [ [1, 1, 1], 0, X, 1 ]
                Array.Copy(source, sourceIndex + maxTargetLength, target, 0, length - maxTargetLength);
            }
            else
            {
                // 5, S: [ [X, 1, 1, 1], 0 ] -> T: [ [A, 0, 0, 0], 0 ]
                //                           -> T: [ [X, 1, 1, 1], 0 ]
                Array.Copy(source, sourceIndex, target, targetIndex, length);
            }
        }
    }
    
    private void Work()
    {
        while (_queue.TryDequeue(out var sample))
        {
            WrapCopy(sample.Audio, 0, this._bigArray, this._bigArrayIndex, sample.Length);
            Interlocked.Add(ref _bigArrayIndex, sample.Length);
            this._floatArrayPool.Return(sample.Audio, true);
        }
    }
    
    /// <summary>
    /// Create a new audio buffer.
    /// </summary>
    /// <param name="length"><see cref="int"/></param>
    /// <returns></returns>
    public float[] NewArray(int length) => this._floatArrayPool.Rent(length);
    
    /// <summary>
    /// Check if the queue has at least a certain amount of audio data.
    /// </summary>
    /// <param name="count"><see cref="int"/></param>
    /// <returns></returns>
    public bool HasAtLeast(int count)
    {
        this.Work();
        return this.Count >= count;
    }

    /// <summary>
    /// Fill a target buffer with audio data and remove it from the queue.
    /// </summary>
    /// <param name="target"><see cref="float"/>[]</param>
    public void Fill(float[] target)
    {
        this.Work();
        WrapCopy(this._bigArray, this._bigArrayOffset, target, 0, target.Length);
        Interlocked.Add(ref _bigArrayOffset, target.Length);
    }
    
    /// <summary>
    /// Add a sample to the queue.
    /// </summary>
    /// <param name="sample"><see cref="Sample"/></param>
    public void Add(Sample sample)
    {
        this._queue.Enqueue(sample);
        this.Work();
    }
    
    /// <summary>
    /// The total active length of the queue.
    /// </summary>
    public int Count => this._bigArrayIndex - this._bigArrayOffset;
    
    /// <summary>
    /// Free an audio buffer.
    /// </summary>
    /// <param name="audioBuf"><see cref="float"/>[]</param>
    public void FreeArray(float[] audioBuf) => this._floatArrayPool.Return(audioBuf, true);
}