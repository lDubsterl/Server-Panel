import React from 'react';
import { LockOutlined, UserOutlined } from '@ant-design/icons';
import { Button, Checkbox, Form, Input, Flex, message } from 'antd';
import { Link } from 'react-router-dom';
import { ApiConfig, parseJwt } from '../services/api';


const Login = () => {
    const [messageApi, contextHolder] = message.useMessage();

    const onFinish = values => {
        ApiConfig.api.post(ApiConfig.authenticationController + '/Login', values)
            .then(response => {
                localStorage.setItem('accessToken', response.data.data.accessToken);
                localStorage.setItem('refreshToken', response.data.data.refreshToken);
                localStorage.setItem('id', parseJwt(response.data.data.accessToken).nameid);
                window.location.href = '/';
            })
            .catch((error) => messageApi.error(
                error?.response?.data
                // JSON.stringify(error)
            ));
    };

    return (
        <div style={{ height: '90vh', display: 'flex', justifyContent: 'center', alignItems: 'center', }} >
            {contextHolder}
            <Form
                name="login"
                initialValues={{ remember: true }}
                style={{ maxWidth: 640, backgroundImage: 'linear-gradient(rgba(255, 255, 255, 0.5), rgba(255, 255, 255, 0.5))', padding: '15px', borderRadius: '10px' }}
                onFinish={onFinish}
            >
                <Form.Item
                    name="email"
                    rules={[{ required: true, message: 'Введите электронную почту' }]}
                >
                    <Input prefix={<UserOutlined />} placeholder="Почта" />
                </Form.Item>
                <Form.Item
                    name="password"
                    rules={[{ required: true, message: 'Введите пароль' }]}
                >
                    <Input prefix={<LockOutlined />} type="password" placeholder="Пароль" />
                </Form.Item>
                <Form.Item>
                    <Flex justify="space-between" align="center">
                        <Form.Item name="remember" valuePropName="checked" noStyle>
                            <Checkbox>Запомнить меня</Checkbox>
                        </Form.Item>
                        <a href="">Забыли пароль?</a>
                    </Flex>
                </Form.Item>

                <Form.Item>
                    <Button block type="primary" htmlType="submit" style={{ backgroundColor: '#333' }}>
                        Вход
                    </Button>
                    или <Link to="/register">Регистрация</Link>
                </Form.Item>
            </Form>
        </div>
    );
};
export default Login;