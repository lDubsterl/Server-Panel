import React from 'react';
import { LockOutlined, UserOutlined } from '@ant-design/icons';
import { Button, Form, Input } from 'antd';
import { useNavigate } from 'react-router-dom';
import ApiConfig from '../services/api';

const Register = () => {
    const navigate = useNavigate();

    const onFinish = values => {
        const now = new Date();
        values.ts = now.toISOString();
        values.role = 'user';
        ApiConfig.api.post(ApiConfig.authenticationController + '/SignUp', values)
            .then(response => navigate('/login'));
    };

    return (
        <div style={{ height: '90vh', display: 'flex', justifyContent: 'center', alignItems: 'center', }} >
            <Form
                name="register"
                style={{ minWidth: '350px', backgroundImage: 'linear-gradient(rgba(255, 255, 255, 0.5), rgba(255, 255, 255, 0.5))', padding: '15px', borderRadius: '10px' }}
                onFinish={onFinish}
            >
                <Form.Item
                    name="email"
                    rules={[{ required: true, message: 'Введите электронную почту' }]}
                >
                    <Input prefix={<UserOutlined />} placeholder="Почта" />
                </Form.Item>
                <Form.Item name='password'
                    rules={[
                        { required: true, message: 'Введите пароль' },
                        {
                            validator(_, value) {
                                if (!value) return Promise.resolve();

                                const hasLetter = /[a-zA-Z]/.test(value);
                                const hasDigit = /\d/.test(value);
                                const isLongEnough = value.length >= 7;

                                if (!hasLetter)
                                    return Promise.reject(new Error('Пароль должен содержать хотя бы одну букву'));
                                if (!hasDigit)
                                    return Promise.reject(new Error('Пароль должен содержать хотя бы одну цифру'));
                                if (!isLongEnough)
                                    return Promise.reject(new Error('Пароль должен быть не короче 7 символов'));

                                return Promise.resolve();
                            }
                        }]}>
                    <Input.Password prefix={<LockOutlined />} allowClear variant='outlined' placeholder="Пароль" />
                </Form.Item>
                <Form.Item name='confirmPassword'
                    dependencies={['password']}
                    rules={[{ required: true, message: 'Повторите пароль', },
                    ({ getFieldValue }) => ({
                        validator(_, value) {
                            if (!value || value === getFieldValue('password')) {
                                return Promise.resolve();
                            }
                            return Promise.reject(new Error('Введённые пароли не совпадают'));
                        }
                    })
                    ]} >
                    <Input.Password prefix={<LockOutlined />} allowClear variant='outlined' placeholder="Повторите пароль" />
                </Form.Item>

                <Form.Item>
                    <Button block type="primary" htmlType="submit" style={{ backgroundColor: '#333' }}>
                        Регистрация
                    </Button>
                </Form.Item>
            </Form>
        </div >
    );
};
export default Register;