cmake_minimum_required(VERSION 2.8.12)
project (EltraSds011)

if (NOT CMAKE_BUILD_TYPE) 
	set (CMAKE_BUILD_TYPE Release)
endif ()

set(DHT22_PATH ${CMAKE_SOURCE_DIR}/)

add_library(${PROJECT_NAME} SHARED ${DHT22_PATH}/sds.cpp
								   ${DHT22_PATH}/sys_helper.cpp
								   ${DHT22_PATH}/sds/sds_helper.cpp
								   ${DHT22_PATH}/sds/sds011_lib.cpp
								   ${DHT22_PATH}/sds/serial.cpp)

target_link_libraries(${PROJECT_NAME} -lrt)

install(TARGETS ${PROJECT_NAME} LIBRARY DESTINATION /usr/lib)