# -*- coding: utf-8 -*-
import sys
from PySide import QtCore, QtGui, QtOpenGL
from OpenGL import GL
from array import *
from ctypes import c_void_p
import math
import time


class GLDemo(QtOpenGL.QGLWidget):

    def __init__(self, parent=None):
        super(GLDemo, self).__init__(parent)
        self.vertices = (
            0.5, 0.5, 0.0, 1.0, 0.0, 0.0,
            0.5, -0.5, 0.0, 0.0, 1.0, 0.0,
            -0.5, -0.5, 0.0, 0.0, 0.0, 1.0,
            -0.5, 0.5, 0.0, 0.0, 0.0, 0.0
        )
        self.indeics = (
            0, 1, 3,
            1, 2, 3
        )
        self.program = None
        self.vertex_arrays = None
        self.color = None
        self.trolltechGreen = QtGui.QColor.fromCmykF(0.40, 0.0, 1.0, 0.0)
        self.trolltechPurple = QtGui.QColor.fromCmykF(0.39, 0.39, 0.0, 0.0)

        self.timer = QtCore.QTimer()
        self.timer.timeout.connect(self.update)
        self.timer.setInterval(10)
        self.timer.start()

    def initializeGL(self):
        self.setup_render_pipeline()
        # GL.glPolygonMode(GL.GL_FRONT_AND_BACK, GL.GL_LINE)
        self.color = GL.glGetUniformLocation(self.program, 'inputColor')
        self.qglClearColor(self.trolltechPurple.darker())

    def setup_render_pipeline(self):
        vertex_buffer = Buffer.generate_buffer(
            self.context(), QtOpenGL.QGLBuffer.VertexBuffer, array('f', self.vertices).tostring())
        element_buffer = Buffer.generate_buffer(
            self.context(), QtOpenGL.QGLBuffer.IndexBuffer, array('I', self.indeics).tostring())
        self.vertex_arrays = self.create_and_int_array_attrib(vertex_buffer, element_buffer)
        vertex_shader = self.create_vertex_shader()
        fragment_shader = self.create_fragment_shader()
        self.program = self.create_shader_program(vertex_shader, fragment_shader)

        GL.glDeleteShader(vertex_shader)
        GL.glDeleteShader(fragment_shader)

    def create_and_int_array_attrib(self, vertex_buffer, element_buffer):
        vertex_arrays = GL.glGenVertexArrays(1)
        GL.glBindVertexArray(vertex_arrays)
        vertex_buffer.bind()
        element_buffer.bind()
        GL.glVertexAttribPointer(0, 3, GL.GL_FLOAT, GL.GL_FALSE, 6 * GL.sizeof(GL.GLfloat), c_void_p(0))
        GL.glVertexAttribPointer(1, 3, GL.GL_FLOAT, GL.GL_FALSE, 6 * GL.sizeof(GL.GLfloat), c_void_p(3 * GL.sizeof(GL.GLfloat)))
        GL.glEnableVertexAttribArray(0)
        GL.glEnableVertexAttribArray(1)
        GL.glBindVertexArray(0)
        return vertex_arrays

    def create_shader_program(self, fragment_shader, vertex_shader):
        program = GL.glCreateProgram()
        GL.glAttachShader(program, vertex_shader)
        GL.glAttachShader(program, fragment_shader)
        GL.glLinkProgram(program)
        return program

    def create_fragment_shader(self):
        shader = GL.glCreateShader(GL.GL_FRAGMENT_SHADER)
        with open('python/fragment_shader.gls') as shader_source:
            GL.glShaderSource(shader, shader_source)
            GL.glCompileShader(shader)
        return shader

    def create_vertex_shader(self):
        vertex_shader = GL.glCreateShader(GL.GL_VERTEX_SHADER)
        with open('python/vertex_shader.gls') as shader_source:
            GL.glShaderSource(vertex_shader, shader_source)
            GL.glCompileShader(vertex_shader)
        return vertex_shader

    def resizeGL(self, width, height):
        GL.glViewport(0, 0, width, height)

    def paintGL(self):

        mat = [
            math.cos(time.time() % math.pi), -math.sin(time.time() % math.pi), 0.0, 0.0,
            math.sin(time.time() % math.pi), math.cos(time.time() % math.pi), 0.0, 0.0,
            0.0, 0.0, 1.0, 0.0,
            0.0, 0.0, 0.0, 1.0
        ]

        GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT)
        GL.glUseProgram(self.program)
        GL.glBindVertexArray(self.vertex_arrays)
        GL.glUniformMatrix4fv(
            GL.glGetUniformLocation(self.program, 'transform'), 1, GL.GL_FALSE, array('f', mat).tostring())
        GL.glDrawElements(GL.GL_TRIANGLES, 6, GL.GL_UNSIGNED_INT, c_void_p(0))
        GL.glBindVertexArray(0)


class Buffer(object):

    def __init__(self, context):
        try:
            super(Buffer, self).__init__(context)
        except TypeError:
            super(Buffer, self).__init__()

        self.__context = context
        self.__bufferID = 0
        self.__usagePattern = QtOpenGL.QGLBuffer.StaticDraw
        self.__type = QtOpenGL.QGLBuffer.VertexBuffer
        self.__isCreated = False

    def create(self, buffer_type):
        self.__assert_context()
        if self.__isCreated:
            return
        try:
            self.__bufferID = GL.glGenBuffers(1)
            self.__type = buffer_type
            self.__isCreated = True
        except Exception:
            raise RuntimeError("Unknown error")

    def bind(self):
        self.__assert_context()
        GL.glBindBuffer(self.__select_buffer_target(), self.__bufferID)

    def unbind(self):
        self.__assert_context()
        GL.glBindBuffer(self.__select_buffer_target(), 0)

    def __select_buffer_target(self):
        if self.__type == QtOpenGL.QGLBuffer.VertexBuffer:
            return GL.GL_ARRAY_BUFFER
        if self.__type == QtOpenGL.QGLBuffer.IndexBuffer:
            return GL.GL_ELEMENT_ARRAY_BUFFER

    def __assert_context(self):
        if not self.__context.isValid():
            raise RuntimeError("Context not valid")

    def __select_memory_usage(self):
        if self.__usagePattern == QtOpenGL.QGLBuffer.StaticDraw:
            return GL.GL_STATIC_DRAW
        if self.__usagePattern == QtOpenGL.QGLBuffer.StaticCopy:
            return GL.GL_STATIC_COPY
        if self.__usagePattern == QtOpenGL.QGLBuffer.StaticRead:
            return GL.GL_STATIC_READ
        if self.__usagePattern == QtOpenGL.QGLBuffer.StreamDraw:
            return GL.GL_STREAM_DRAW
        if self.__usagePattern == QtOpenGL.QGLBuffer.StreamCopy:
            return GL.GL_STREAM_COPY
        if self.__usagePattern == QtOpenGL.QGLBuffer.StreamRead:
            return GL.GL_STREAM_READ
        if self.__usagePattern == QtOpenGL.QGLBuffer.DynamicDraw:
            return GL.GL_DYNAMIC_DRAW
        if self.__usagePattern == QtOpenGL.QGLBuffer.DynamicCopy:
            return GL.GL_DYNAMIC_COPY
        if self.__usagePattern == QtOpenGL.QGLBuffer.DynamicRead:
            return GL.GL_DYNAMIC_READ

    def buffer_id(self):
        return self.__bufferID

    def allocate(self, arg=None):
        self.__assert_context()
        if not self.__isCreated:
            self.create(self.__type)
        self.bind()
        if arg is not None:
            if isinstance(arg, int):
                GL.glBufferData(self.__select_buffer_target(), arg, c_void_p(0), self.__select_memory_usage())
            elif isinstance(arg, str):
                GL.glBufferData(self.__select_buffer_target(), arg, self.__select_memory_usage())
        else:
            GL.glBufferData(self.__select_buffer_target(), 0, c_void_p(0), self.__select_memory_usage())
        self.unbind()

    @staticmethod
    def generate_buffer(context, buffer_type, arg):
        ret_buffer = Buffer(context)
        ret_buffer.create(buffer_type)
        ret_buffer.allocate(arg)
        return ret_buffer


if __name__ == '__main__':
    app = QtGui.QApplication(sys.argv)
    demo = GLDemo()
    demo.show()
    sys.exit(app.exec_())
