cmake_minimum_required(VERSION 3.18)
project(gu_native C)

set(CMAKE_C_STANDARD 11)
set(CMAKE_C_STANDARD_REQUIRED ON)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

# Shared common types (header-only)
add_library(gu_native_common INTERFACE)
target_include_directories(gu_native_common
    INTERFACE ${CMAKE_CURRENT_SOURCE_DIR}/gu_native_common/include)

# Option: build with CUDA support
option(GU_ENABLE_CUDA "Build with CUDA kernel support" OFF)

# gu_cuda_core -- the library that C# P/Invoke calls
add_library(gu_cuda_core SHARED
    gu_cuda_core/src/gu_cuda_core.c)

target_link_libraries(gu_cuda_core PRIVATE gu_native_common)
target_include_directories(gu_cuda_core
    PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/gu_cuda_core/include
    PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/gu_cuda_kernels/include)

if(GU_ENABLE_CUDA)
    enable_language(CUDA)
    set(CMAKE_CUDA_STANDARD 17)
    set(CMAKE_CUDA_STANDARD_REQUIRED ON)

    target_compile_definitions(gu_cuda_core PRIVATE GU_HAS_CUDA=1)

    # Add CUDA kernel sources -- compiled by nvcc
    target_sources(gu_cuda_core PRIVATE
        gu_cuda_kernels/src/gu_cuda_kernels.cu)
    target_include_directories(gu_cuda_core PRIVATE
        ${CMAKE_CURRENT_SOURCE_DIR}/gu_cuda_kernels/include)

    # CUDA architecture targets (compute capability 8.0+ for Ampere and newer)
    # CUDA 13.x dropped support for compute_70/75 (Volta/Turing)
    set_target_properties(gu_cuda_core PROPERTIES
        CUDA_ARCHITECTURES "80;86;89;90")
else()
    # Without CUDA, compile the .cu file as plain C for CPU-fallback testing.
    # Rename extension so CMake compiles it as C.
    configure_file(
        ${CMAKE_CURRENT_SOURCE_DIR}/gu_cuda_kernels/src/gu_cuda_kernels.cu
        ${CMAKE_CURRENT_BINARY_DIR}/gu_cuda_kernels_cpu.c
        COPYONLY)
    target_sources(gu_cuda_core PRIVATE
        ${CMAKE_CURRENT_BINARY_DIR}/gu_cuda_kernels_cpu.c)
    target_include_directories(gu_cuda_core PRIVATE
        ${CMAKE_CURRENT_SOURCE_DIR}/gu_cuda_kernels/include)
endif()

# Math library (needed for sqrt on some platforms)
target_link_libraries(gu_cuda_core PRIVATE m)

# gu_phase2_cuda -- Phase II Jacobian/Hessian/batch kernels
if(GU_ENABLE_CUDA)
    add_library(gu_phase2_cuda SHARED
        gu_phase2_cuda/src/gu_phase2_init.cu
        gu_phase2_cuda/src/jacobian_actions.cu
        gu_phase2_cuda/src/hessian_actions.cu
        gu_phase2_cuda/src/branch_kernels.cu
        gu_phase2_cuda/src/spectrum_probes.cu
        gu_phase2_cuda/src/continuation_kernels.cu)
else()
    configure_file(
        ${CMAKE_CURRENT_SOURCE_DIR}/gu_phase2_cuda/src/gu_phase2_init.cu
        ${CMAKE_CURRENT_BINARY_DIR}/gu_phase2_init_cpu.c
        COPYONLY)
    configure_file(
        ${CMAKE_CURRENT_SOURCE_DIR}/gu_phase2_cuda/src/jacobian_actions.cu
        ${CMAKE_CURRENT_BINARY_DIR}/gu_phase2_jacobian_actions_cpu.c
        COPYONLY)
    configure_file(
        ${CMAKE_CURRENT_SOURCE_DIR}/gu_phase2_cuda/src/hessian_actions.cu
        ${CMAKE_CURRENT_BINARY_DIR}/gu_phase2_hessian_actions_cpu.c
        COPYONLY)
    configure_file(
        ${CMAKE_CURRENT_SOURCE_DIR}/gu_phase2_cuda/src/branch_kernels.cu
        ${CMAKE_CURRENT_BINARY_DIR}/gu_phase2_branch_kernels_cpu.c
        COPYONLY)
    configure_file(
        ${CMAKE_CURRENT_SOURCE_DIR}/gu_phase2_cuda/src/spectrum_probes.cu
        ${CMAKE_CURRENT_BINARY_DIR}/gu_phase2_spectrum_probes_cpu.c
        COPYONLY)
    configure_file(
        ${CMAKE_CURRENT_SOURCE_DIR}/gu_phase2_cuda/src/continuation_kernels.cu
        ${CMAKE_CURRENT_BINARY_DIR}/gu_phase2_continuation_kernels_cpu.c
        COPYONLY)
    add_library(gu_phase2_cuda SHARED
        ${CMAKE_CURRENT_BINARY_DIR}/gu_phase2_init_cpu.c
        ${CMAKE_CURRENT_BINARY_DIR}/gu_phase2_jacobian_actions_cpu.c
        ${CMAKE_CURRENT_BINARY_DIR}/gu_phase2_hessian_actions_cpu.c
        ${CMAKE_CURRENT_BINARY_DIR}/gu_phase2_branch_kernels_cpu.c
        ${CMAKE_CURRENT_BINARY_DIR}/gu_phase2_spectrum_probes_cpu.c
        ${CMAKE_CURRENT_BINARY_DIR}/gu_phase2_continuation_kernels_cpu.c)
endif()

target_include_directories(gu_phase2_cuda
    PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/gu_phase2_cuda/include)
target_link_libraries(gu_phase2_cuda PRIVATE m)

# gu_vulkan_native -- Vulkan rendering bridge
add_library(gu_vulkan_native SHARED
    gu_vulkan_native/src/gu_vulkan_bridge.c)

target_include_directories(gu_vulkan_native
    PUBLIC ${CMAKE_CURRENT_SOURCE_DIR}/gu_vulkan_native/include)

# Link Vulkan and math libraries
find_package(Vulkan)
if(Vulkan_FOUND)
    target_link_libraries(gu_vulkan_native PRIVATE Vulkan::Vulkan m)
    target_compile_definitions(gu_vulkan_native PRIVATE GU_HAS_VULKAN=1)
else()
    message(WARNING "Vulkan not found - gu_vulkan_native will build without GPU rendering")
    target_link_libraries(gu_vulkan_native PRIVATE m)
endif()

# Install rules
install(TARGETS gu_cuda_core gu_phase2_cuda gu_vulkan_native
    LIBRARY DESTINATION lib
    RUNTIME DESTINATION bin)
