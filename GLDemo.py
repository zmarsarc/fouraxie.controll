# -*- coding: utf-8 -*-
import sys
from PySide import QtCore, QtGui, QtOpenGL
from OpenGL import GL
from array import *
from ctypes import c_void_p, c_float, sizeof
import math
import time


class GLDemo(QtOpenGL.QGLWidget):

    def __init__(self, parent=None):
        super(GLDemo, self).__init__(parent)
        self.vertices = (
            0.5, 0.5, 0.0,
            0.5, -0.5, 0.0,
            -0.5, -0.5, 0.0,
            -0.5, 0.5, 0.0
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
        vertex_buffer = self.create_and_init_buffer()
        element_buffer = self.create_element_buffer()
        self.vertex_arrays = self.create_and_int_array_attrib(vertex_buffer, element_buffer)
        vertex_shader = self.create_vertex_shader()
        fragment_shader = self.create_fragment_shader()
        self.program = self.create_shader_program(vertex_shader, fragment_shader)

        GL.glDeleteShader(vertex_shader)
        GL.glDeleteShader(fragment_shader)

    def create_element_buffer(self):
        ebo = GL.glGenBuffers(1)
        GL.glBindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, ebo)
        GL.glBufferData(GL.GL_ELEMENT_ARRAY_BUFFER, array('I', self.indeics).tostring(), GL.GL_STATIC_DRAW)
        GL.glBindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, 0)
        return ebo

    def create_and_int_array_attrib(self, vertex_buffer, element_buffer):
        vertex_arrays = GL.glGenVertexArrays(1)
        GL.glBindVertexArray(vertex_arrays)
        GL.glBindBuffer(GL.GL_ARRAY_BUFFER, vertex_buffer)
        GL.glBindBuffer(GL.GL_ELEMENT_ARRAY_BUFFER, element_buffer)
        GL.glVertexAttribPointer(0, 3, GL.GL_FLOAT, GL.GL_FALSE, 3 * sizeof(c_float), c_void_p(0))
        GL.glEnableVertexAttribArray(0)
        GL.glBindVertexArray(0)
        return vertex_arrays

    def create_and_init_buffer(self):
        vertex_buffer = GL.glGenBuffers(1)
        GL.glBindBuffer(GL.GL_ARRAY_BUFFER, vertex_buffer)
        GL.glBufferData(GL.GL_ARRAY_BUFFER, array('f', self.vertices).tostring(), GL.GL_STATIC_DRAW)
        GL.glBindBuffer(GL.GL_ARRAY_BUFFER, 0)
        return vertex_buffer

    def create_shader_program(self, fragment_shader, vertex_shader):
        program = GL.glCreateProgram()
        GL.glAttachShader(program, vertex_shader)
        GL.glAttachShader(program, fragment_shader)
        GL.glLinkProgram(program)
        return program

    def create_fragment_shader(self):
        shader = GL.glCreateShader(GL.GL_FRAGMENT_SHADER)
        with open('fragment_shader.gls') as shader_source:
            GL.glShaderSource(shader, shader_source)
            GL.glCompileShader(shader)
        return shader

    def create_vertex_shader(self):
        vertex_shader = GL.glCreateShader(GL.GL_VERTEX_SHADER)
        with open('vertex_shader.gls') as shader_source:
            GL.glShaderSource(vertex_shader, shader_source)
            GL.glCompileShader(vertex_shader)
        return vertex_shader

    def resizeGL(self, width, height):
        GL.glViewport(0, 0, width, height)

    def paintGL(self):
        GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT)
        GL.glUseProgram(self.program)
        GL.glBindVertexArray(self.vertex_arrays)
        GL.glUniform4f(self.color, math.cos(time.time() % math.pi), math.sin(time.time() % math.pi), math.tan(time.time() % math.pi), 1.0)
        GL.glDrawElements(GL.GL_TRIANGLES, 6, GL.GL_UNSIGNED_INT, c_void_p(0))
        GL.glBindVertexArray(0)

if __name__ == '__main__':
    app = QtGui.QApplication(sys.argv)
    demo = GLDemo()
    demo.show()
    sys.exit(app.exec_())
