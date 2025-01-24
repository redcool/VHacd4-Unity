project(VHACD_DLL)
include(${CMAKE_COMMON_INC})

if (NOT NO_OPENCL)
	find_package(OpenCL QUIET)
endif()
if (NOT NO_OPENMP)
	find_package(OpenMP QUIET)
endif()
if(OPENMP_FOUND)
    set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} ${OpenMP_C_FLAGS}")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} ${OpenMP_CXX_FLAGS}")
    set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} ${OpenMP_EXE_LINKER_FLAGS}")
endif()

add_library(libvhacd SHARED ${PROJECT_CPP_FILES} ${PROJECT_C_FILES} ${PROJECT_INC_FILES} ${PROJECT_INL_FILES})

target_include_directories(libvhacd PRIVATE ${CMAKE_SOURCE_DIR}/VHACD_Lib/include)
add_definitions( -D ENABLE_VHACD_IMPLEMENTATION)

if (NOT WIN32 AND NOT APPLE)
   set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -g -Wall")
   target_link_libraries(libvhacd rt)
endif()

