cmake_minimum_required(VERSION 3.5)

project(learnopengl LANGUAGES CXX C)

set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

include_directories(${CMAKE_SOURCE_DIR}/include)

#add_executable(learnopengl main-rgb-rgb.cpp glad.c shader_s.h stb_image.h stb_image.cpp)
add_executable(learnopengl main-yuv-yuv.cpp glad.c shader_s.h stb_image.h stb_image.cpp)

target_link_libraries(learnopengl ${CMAKE_SOURCE_DIR}/lib/glfw3.lib opengl32.lib)
