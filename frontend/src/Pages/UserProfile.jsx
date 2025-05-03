import { useEffect, useState } from "react";
import ApiConfig from "../services/api";
import { Form, Input, Button, Flex, Card, message } from "antd";

const UserProfile = () => {
    const [form] = Form.useForm();
    const [messageApi, contextHolder] = message.useMessage();

    const [ftpCreds, setFtpCreds] = useState("");
    const [email, setEmail] = useState("");
    const [registrationDate, setRegDate] = useState("");

    useEffect(() => {
        ApiConfig.api
            .get(`/${ApiConfig.userController}/GetUserInfo`)
            .then((response) => {
                setFtpCreds(response.data.ftpCreds);
                setEmail(response.data.email);
                const date = new Date(response.data.registrationDate);
                const formattedDate = date.toLocaleString('ru-RU', {
                    day: '2-digit',
                    month: '2-digit',
                    year: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit'
                }).replace(',', '');
                setRegDate(formattedDate);
            });
    })

    const handlePasswordChange = () => {
        const fields = form.getFieldsValue();
        ApiConfig.api
            .post(`/${ApiConfig.userController}/ChangePassword`, fields)
            .then(() => messageApi.info("Пароль успешно изменён"))
            .catch((error) => messageApi.error(error.response.data));
    };

    return (
        <Flex vertical align="center" style={{ marginTop: '20px' }}>
            {contextHolder}
            <Card title="Основная информация" style={{ width: '100%', maxWidth: 600, marginBottom: 24 }} styles={{ body: { textAlign: 'start' } }}>
                <p>Электронная почта: {email}</p>
                <p>Данные для подключения по FTP: {ftpCreds}</p>
                <p>Дата регистрации: {registrationDate}</p>
            </Card>
            <Card title="Смена пароля" style={{ width: '100%', maxWidth: 600 }}>
                <Form form={form} size="medium" layout="vertical" wrapperCol={{ span: 24 }}
                    onFinish={handlePasswordChange}>
                    <Form.Item label='Текущий пароль' name='currentPassword'>
                        <Input.Password allowClear variant='outlined' />
                    </Form.Item>
                    <Form.Item label='Новый пароль' name='newPassword'
                        rules={[{
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
                        <Input.Password allowClear variant='outlined' />
                    </Form.Item>
                    <Form.Item label='Подтверждение пароля' name='confirmPassword'
                        dependencies={['newPassword']}
                        rules={[({ getFieldValue }) => ({
                            validator(_, value) {
                                if (value === getFieldValue('confirmPassword'))
                                    return Promise.resolve();
                                return Promise.reject(new Error('Введённые пароли не совпадают'));
                            }
                        })]}>
                        <Input.Password allowClear variant='outlined' />
                    </Form.Item>
                    <Form.Item>
                        <Button type="primary" htmlType="submit" style={{ backgroundColor: '#333', marginTop: '10px', width: '100%' }}>
                            Изменить пароль
                        </Button>
                    </Form.Item>
                </Form>
            </Card>
        </Flex>
    );
};

export default UserProfile;
