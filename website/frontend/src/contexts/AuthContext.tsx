/* eslint-disable react-refresh/only-export-components */
import { createContext, useContext, useState, ReactNode } from 'react'
import api from '@/services/api'

export type UserRole = "direcao" | "professor" | "encarregado"

export interface User {
    id: string
    nome: string
    email: string
    tipo: UserRole
}

interface AuthContextType {
    user: User | null
    isAuthenticated: boolean
    login: (email: string, password: string) => Promise<boolean>
    register: (data: RegisterData) => Promise<boolean>
    logout: () => void
}

interface RegisterData {
    nome: string
    email: string
    password: string
    tipo: number
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export const AuthProvider = ({ children }: { children: ReactNode }) => {
    const [user, setUser] = useState<User | null>(() => {
        const storedUser = localStorage.getItem('user')
        const token = localStorage.getItem('token')
        if (storedUser && token) {
            return JSON.parse(storedUser)
        }
        return null
    })

    const login = async (email: string, password: string): Promise<boolean> => {
        try {
            const response = await api.post('/auth/login', { email, password })
            const { token, tipo, nome, id } = response.data
            const roleMap: Record<number, UserRole> = { 0: 'direcao', 1: 'professor', 2: 'encarregado' }
            const userObj: User = {
                id: id || '0',
                nome,
                email,
                tipo: roleMap[tipo],
            }
            localStorage.setItem('token', token)
            localStorage.setItem('user', JSON.stringify(userObj))
            setUser(userObj)
            return true
        } catch (error) {
            console.error('Login failed', error)
            return false
        }
    }

    const register = async (data: RegisterData): Promise<boolean> => {
        try {
            await api.post('/auth/register', data)
            return true
        } catch (error) {
            console.error('Registration failed', error)
            return false
        }
    }

    const logout = () => {
        localStorage.removeItem('token')
        localStorage.removeItem('user')
        setUser(null)
    }

    return (
        <AuthContext.Provider value={{ user, isAuthenticated: !!user, login, register, logout }}>
            {children}
        </AuthContext.Provider>
    )
}

export const useAuth = () => {
    const ctx = useContext(AuthContext)
    if (!ctx) throw new Error('useAuth must be used within AuthProvider')
    return ctx
}