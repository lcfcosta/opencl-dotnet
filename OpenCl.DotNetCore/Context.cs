
#region Using Directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using OpenCl.DotNetCore.Interop;

#endregion

namespace OpenCl.DotNetCore
{
    /// <summary>
    /// Represents an OpenCL context.
    /// </summary>
    public class Context : HandleBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="Context"/> instance.
        /// </summary>
        /// <param name="handle">The handle to the OpenCL context.</param>
        internal Context(IntPtr handle)
            : base(handle)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a program from the provided source code. The program is created, compiled, and linked.
        /// </summary>
        /// <param name="source">The source code from which the program is to be created.</param>
        /// <exception cref="OpenClException">If the program could not be created, compiled, or linked, then an <see cref="OpenClException"/> is thrown.</exception>
        /// <returns>Returns the created program.</returns>
        public Program CreateAndBuildProgramFromString(string source)
        {
            // Loads the program from the specified source string
            Result result;
            IntPtr[] sourceList = new IntPtr[] { Marshal.StringToHGlobalAnsi(source) };
            uint[] sourceLengths = new uint[] { (uint)source.Length };
            IntPtr programPointer = NativeMethods.CreateProgramWithSource(this.Handle, 1, sourceList, sourceLengths, out result);

            // Checks if the program creation was successful, if not, then an exception is thrown
            if (result != Result.Success)
                throw new OpenClException("The program could not be created.", result);

            // Builds (compiles and links) the program and checks if it was successful, if not, then an exception is thrown
            result = NativeMethods.BuildProgram(programPointer, 0, null, null, IntPtr.Zero, IntPtr.Zero);
            if (result != Result.Success)
                throw new OpenClException("The program could not be compiled and linked.", result);

            // Creates the new program and returns it
            Program program = new Program(programPointer);
            return program;
        }

        /// <summary>
        /// Creates a new memory object with the specified flags and of the specified size.
        /// </summary>
        /// <param name="memoryFlags">The flags, that determines the how the memory object is created and how it can be accessed.</param>
        /// <param name="size">The size of memory that should be allocated for the memory object.</param>
        /// <exception cref="OpenClException">If the memory object could not be created, then an <see cref="OpenClException"/> is thrown.</exception>
        /// <returns>Returns the created memory object.</returns>
        public MemoryObject CreateMemoryObject(OpenCl.DotNetCore.MemoryFlag memoryFlags, int size)
        {
            // Creates a new memory object of the specified size and with the specified memory flags
            Result result;
            IntPtr memoryObjectPointer = NativeMethods.CreateBuffer(this.Handle, (OpenCl.DotNetCore.Interop.MemoryFlag)memoryFlags, new UIntPtr((uint)size), IntPtr.Zero, out result);
            
            // Checks if the creation of the memory object was successful, if not, then an exception is thrown
            if (result != Result.Success)
                throw new OpenClException("The memory object could not be created.", result);

            // Creates the memory object from the pointer to the memory object and returns it
            MemoryObject memoryObject = new MemoryObject(memoryObjectPointer);
            return memoryObject;
        }

        /// <summary>
        /// Creates a new memory object with the specified flags. The size of memory allocated for the memory object is determined by <see cref="T"/> and the number of elements.
        /// </summary>
        /// <typeparam name="T">The size of the memory object will be determined by the structure specified in the type parameter.</typeparam>
        /// <param name="memoryFlags">The flags, that determines the how the memory object is created and how it can be accessed.</param>
        /// <exception cref="OpenClException">If the memory object could not be created, then an <see cref="OpenClException"/> is thrown.</exception>
        /// <returns>Returns the created memory object.</returns>
        public MemoryObject CreateMemoryObject<T>(OpenCl.DotNetCore.MemoryFlag memoryFlags, int size) where T : struct => this.CreateMemoryObject(memoryFlags, Marshal.SizeOf<T>() * size);

        /// <summary>
        /// Creates a new memory object with the specified flags for the specified array. The size of memory 1allocated for the memory object is determined by <see cref="T"/> and the number of elements in the array.
        /// </summary>
        /// <typeparam name="T">The size of the memory object will be determined by the structure specified in the type parameter.</typeparam>
        /// <param name="memoryFlags">The flags, that determines the how the memory object is created and how it can be accessed.</param>
        /// <param name="value">The value that is to be copied over to the device.</param>
        /// <exception cref="OpenClException">If the memory object could not be created, then an <see cref="OpenClException"/> is thrown.</exception>
        /// <returns>Returns the created memory object.</returns>
        public MemoryObject CreateMemoryObject<T>(OpenCl.DotNetCore.MemoryFlag memoryFlags, T[] value) where T : struct
        {
            // Tries to create the memory object, if anything goes wrong, then it is crucial to free the allocated memory
            IntPtr hostMemoryObjectPointer = IntPtr.Zero;
            try
            {
                // Determines the size of the specified value and creates a pointer that points to the data inside the structure
                int size = Marshal.SizeOf<T>() * value.Length;
                hostMemoryObjectPointer = Marshal.AllocHGlobal(size);
                for (int i = 0; i < value.Length; i++)
                    Marshal.StructureToPtr(value[i], IntPtr.Add(hostMemoryObjectPointer, i * Marshal.SizeOf<T>()), false);

                // Creates a new memory object for the specified value
                Result result;
                IntPtr memoryObjectPointer = NativeMethods.CreateBuffer(this.Handle, (OpenCl.DotNetCore.Interop.MemoryFlag)memoryFlags, new UIntPtr((uint)size), hostMemoryObjectPointer, out result);

                // Checks if the creation of the memory object was successful, if not, then an exception is thrown
                if (result != Result.Success)
                    throw new OpenClException("The memory object could not be created.", result);

                // Creates the memory object from the pointer to the memory object and returns it
                MemoryObject memoryObject = new MemoryObject(memoryObjectPointer);
                return memoryObject;
            }
            finally
            {
                // Deallocates the host memory allocated for the value
                if (hostMemoryObjectPointer != IntPtr.Zero)
                    Marshal.FreeHGlobal(hostMemoryObjectPointer);
            }
        }

        #endregion
        
        #region Public Static Methods

        /// <summary>
        /// Creates a new context for the specified device.
        /// </summary>
        /// <param name="device">The device for which the context is to be created.</param>
        /// <exception cref="OpenClException">If the context could not be created, then an <see cref="OpenClException"/> exception is thrown.</exception>
        /// <returns>Returns the created context.</returns>
        public static Context CreateContext(Device device) => Context.CreateContext(new List<Device> { device });

        /// <summary>
        /// Creates a new context for the specified device.
        /// </summary>
        /// <param name="devices">The devices for which the context is to be created.</param>
        /// <exception cref="OpenClException">If the context could not be created, then an <see cref="OpenClException"/> exception is thrown.</exception>
        /// <returns>Returns the created context.</returns>
        public static Context CreateContext(IEnumerable<Device> devices)
        {
            // Creates the new context for the specified devices
            Result result;
            IntPtr contextPointer = NativeMethods.CreateContext(null, (uint)devices.Count(), devices.Select(device => device.Handle).ToArray(), IntPtr.Zero, IntPtr.Zero, out result);

            // Checks if the device creation was successful, if not, then an exception is thrown
            if (result != Result.Success)
                throw new OpenClException("The context could not be created.", result);

            // Creates the new context object from the pointer and returns it
            return new Context(contextPointer);
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Disposes of the resources that have been acquired by the context.
        /// </summary>
        /// <param name="disposing">Determines whether managed object or managed and unmanaged resources should be disposed of.</param>
        protected override void Dispose(bool disposing)
        {
            // Checks if the context has already been disposed of, if not, then the context is disposed of
            if (!this.IsDisposed)
                NativeMethods.ReleaseContext(this.Handle);

            // Makes sure that the base class can execute its dispose logic
            base.Dispose(disposing);
        }

        #endregion
    }
}