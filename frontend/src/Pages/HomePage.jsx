import { Card, Button, Typography, Row, Col } from 'antd';
import { RocketOutlined, SettingOutlined, CodeOutlined, ClusterOutlined } from '@ant-design/icons';
import { Link } from 'react-router-dom';
import styles from "../styles/HomePage.module.css";

const { Title, Paragraph } = Typography;

const HomePage = ({ isAuthenticated, id }) => {
    return (
        <div style={{ padding: '40px 20px', maxWidth: 1200, margin: '0 auto' }}>
            <Title level={2} style={{
                textAlign: 'center',
                fontWeight: 700,
                fontSize: '36px',
                marginBottom: '24px',
                color: '#111',
                backgroundColor: '#ffffffcc', // полупрозрачный белый
                padding: '12px 24px',
                borderRadius: '12px',
                boxShadow: '0 4px 12px rgba(0, 0, 0, 0.1)',
                display: 'inline-block'
            }}>Панель управления игровыми серверами</Title>
            <Paragraph
                style={{
                    textAlign: 'center',
                    maxWidth: 800,
                    margin: '0 auto 40px',
                    fontSize: '18px',
                    color: '#222',
                    lineHeight: 1.7,
                    fontWeight: 400,
                    backgroundColor: '#ffffffee', // чуть прозрачный белый
                    padding: '20px 30px',
                    borderRadius: '12px',
                    boxShadow: '0 6px 16px rgba(0, 0, 0, 0.1)'
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
