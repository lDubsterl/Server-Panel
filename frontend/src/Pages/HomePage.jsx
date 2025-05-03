import { Card, Button, Typography, Row, Col } from 'antd';
import { RocketOutlined, SettingOutlined, CodeOutlined, ClusterOutlined } from '@ant-design/icons';
import { Link } from 'react-router-dom';
import styles from "../styles/HomePage.module.css";

const { Title, Paragraph } = Typography;

const HomePage = ({ isAuthenticated, id }) => {
    return (
        <div style={{ padding: '40px 20px', maxWidth: 1200, margin: '0 auto' }}>
            <Title level={2} style={{ textAlign: 'center' }}>Панель управления игровыми серверами</Title>
            <Paragraph
                style={{
                    textAlign: 'center',
                    maxWidth: 800,
                    margin: '0 auto 40px',
                    fontSize: '20px',
                    color: '#333',
                    lineHeight: 1.8,
                    fontWeight: 500,
                    background: 'linear-gradient(45deg, #f5f5f5, #e0e0e0)', // Лёгкий градиент фона
                    padding: '10px 20px',
                    borderRadius: '8px',
                    boxShadow: '0 4px 8px rgba(0, 0, 0, 0.1)', // Тень для контраста
                    fontFamily: 'Arial, sans-serif'
                }}>
                Запускайте, настраивайте и управляйте серверами для <strong>Terraria</strong>, <strong>Don't Starve Together</strong> и <strong>Minecraft</strong> — прямо из браузера. Простая и мощная панель для геймеров и администраторов.
            </Paragraph>

            <Row gutter={[24, 24]} justify="center">
                <Col xs={24} md={8}>
                    <Link to={`/${id}`} >
                        <Card hoverable style={{ textAlign: 'center' }} className={styles.card}>
                            <RocketOutlined style={{ fontSize: 40, marginBottom: 16 }} />
                            <Title level={4}>Быстрый запуск</Title>
                            <Paragraph>Запускайте сервер в пару кликов без лишней настройки.</Paragraph>
                        </Card>
                    </Link>
                </Col>
                <Col xs={24} md={8}>
                    <Link to={`/${id}`} >
                        <Card hoverable style={{ textAlign: 'center' }} className={styles.card}>
                            <SettingOutlined style={{ fontSize: 40, marginBottom: 16 }} />
                            <Title level={4}>Удобная настройка</Title>
                            <Paragraph>Интерфейс позволяет редактировать конфигурации в человеко-читаемом виде.</Paragraph>
                        </Card>
                    </Link>
                </Col>
                <Col xs={24} md={8}>
                    <Link to={`/${id}`} >
                        <Card hoverable style={{ textAlign: 'center' }} className={styles.card}>
                            <CodeOutlined style={{ fontSize: 40, marginBottom: 16 }} />
                            <Title level={4}>Прямая консоль</Title>
                            <Paragraph>Просматривайте логи и отправляйте команды в реальном времени.</Paragraph>
                        </Card>
                    </Link>
                </Col>
                <Col xs={24} md={8}>
                    <Link to={`/${id}`} >
                        <Card hoverable style={{ textAlign: 'center' }} className={styles.card}>
                            <ClusterOutlined style={{ fontSize: 40, marginBottom: 16 }} />
                            <Title level={4}>Кластеры серверов</Title>
                            <Paragraph>Управляйте несколькими серверами одновременно без затруднений</Paragraph>
                        </Card>
                    </Link>
                </Col>
            </Row>

            <div style={{ textAlign: 'center', marginTop: 50 }}>
                {!isAuthenticated ? <><Link to="/Register">
                    <Button type="primary" size="large" style={{ marginRight: 16, backgroundColor: '#333' }}>
                        Начать бесплатно
                    </Button>
                </Link>
                    <Link to="/Login">
                        <Button type="primary" size="large" style={{ backgroundColor: '#333' }}>Вход</Button>
                    </Link></> :
                    <Link to={`/${id}`} >
                        <Button type="primary" size="large" style={{ marginRight: 16, backgroundColor: '#333' }}>
                            Перейти к панели управления
                        </Button>
                    </Link>}
            </div>
        </div >
    );
};

export default HomePage;
