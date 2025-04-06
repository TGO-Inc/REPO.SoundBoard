using System.Collections.Concurrent;
using SoundBoard.Models.Audio;

namespace SoundBoard.Models.Collections;

/// <summary>
///     Fully thread-safe fast queue for audio data.
/// </summary>
public class FastQueue(FastMemory memManager)
{
    private readonly float[] _bigArray = new float[short.MaxValue];
    private readonly ConcurrentQueue<AudioSample> _queue = [];
    private long _bigArrayIndex;
    private long _bigArrayOffset;

    /// <summary>
    ///     The total active length of the queue.
    /// </summary>
    public long Count => _bigArrayIndex - _bigArrayOffset;

    private static void WrapCopy(
        float[] source,
        long sourceIndex,
        float[] target,
        long targetIndex,
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

    private void IncrementIndex(int amt)
    {
        Interlocked.Add(ref _bigArrayIndex, amt);
    }

    private void IncrementOffset(int amt)
    {
        Interlocked.Add(ref _bigArrayOffset, amt);
    }

    private void Work()
    {
        while (_queue.TryDequeue(out var sample))
        {
            WrapCopy(sample.Audio, 0, _bigArray, _bigArrayIndex, sample.Length);
            IncrementIndex(sample.Length);
            memManager.FreeArray(sample.Audio);
        }
    }

    /// <summary>
    ///     Check if the queue has at least a certain amount of audio data.
    /// </summary>
    /// <param name="count">
    ///     <see cref="int" />
    /// </param>
    /// <returns></returns>
    public bool HasAtLeast(int count)
    {
        Work();
        return Count >= count;
    }

    /// <summary>
    ///     Fill a target buffer with audio data and remove it from the queue.
    /// </summary>
    /// <param name="target"><see cref="float" />[]</param>
    public void Fill(float[] target)
    {
        Work();
        WrapCopy(_bigArray, _bigArrayOffset, target, 0, target.Length);
        IncrementOffset(target.Length);
    }

    /// <summary>
    ///     Add a sample to the queue.
    /// </summary>
    /// <param name="audioSample">
    ///     <see cref="AudioSample" />
    /// </param>
    public void Add(AudioSample audioSample)
    {
        _queue.Enqueue(audioSample);
        Work();
    }
}